using UnityEngine;
using UnityEngine.InputSystem;

public class Grapple : MonoBehaviour
{
    //Hand tracking
    public Transform[] Hands;
    private Vector3[] HandPos;
    private Quaternion[] HandRot;

    //Prefabs
    public InputActionReference gripActionR;
    public InputActionReference gripActionL;
    public MeshFilter[] meshes;
    public GameObject[] reticle;
    public GameObject projectile;
    public GameObject player;

    //Editor Values
    public float fireSpd;
    public float pullSpd;
    private float tugSpd;
    public float triggerSensitivity;
    public float grappleLength;
    public float severanceRange;
    public float ropeThickness;

    //States
    private bool[] grips;
    private bool[] hooked;
    private bool[] held;
    private bool[] pull;

    //Privates
    private Vector3 respawn;
    private GameObject[] grabbed;
    private Vector3[] dist;
    private GameObject[] current;
    private Vector3 lastHit;
    private readonly Vector3[] zeroed = new Vector3[] {Vector3.down, Vector3.down};

    private Mesh[] ropes;
    private Vector3[] RVerts;
    private Vector3[] LVerts;
    private Vector2[] RUvs;
    private Vector2[] LUvs;
    private readonly int[] Tris = new int[] { 0, 4, 1, 4, 5, 1, 1, 5, 2, 5, 6, 2, 2, 6, 3, 6, 7, 3, 3, 7, 0, 7, 4, 0, 0, 1, 3, 1, 2, 3 };

    void Awake()
    {
        HandPos = new Vector3[2];
        HandRot = new Quaternion[2];

        held = new bool[2];
        pull = new bool[2];
        grips = new bool[2];
        hooked = new bool[2];
        dist = new Vector3[2];
        current = new GameObject[2];
        grabbed = new GameObject[2];

        ropes = new Mesh[2];
        RVerts = new Vector3[8];
        LVerts = new Vector3[8];
        RUvs = new Vector2[8];
        LUvs = new Vector2[8];

        dist = new Vector3[2];
        triggerSensitivity = Mathf.Clamp01(triggerSensitivity);
        tugSpd = pullSpd / 6;

        respawn = transform.position;
    }

    private void Update()
    {
        //Read inputs
        Fire(gripActionR.action, 0);
        Fire(gripActionL.action, 1);

        //Track hand position
        for (int i = 0; i < 2; i++) {
            HandPos[i] = Hands[i].position;
            HandRot[i] = Hands[i].rotation;
            HandVertex(i);
        }

        //Reticle for player aim
        for (int i = 0; i < 2; i++) {
            if (pull[i]) reticle[i].SetActive(false);
            if (!grips[i]) {
                if (Physics.Raycast(HandPos[i], Hands[i].forward, out RaycastHit hit, grappleLength, 3, QueryTriggerInteraction.Ignore)) {
                    reticle[i].SetActive(true);
                    reticle[i].transform.position = hit.point;
                    reticle[i].transform.rotation = Quaternion.LookRotation(-hit.normal);
                } else {
                    reticle[i].SetActive(false);
                }
            }
        }

        //Check both hands for grappling hooks
        for (int i = 0; i < 2; i++) {
            if (current[i] != null) {
                //Distance between player and hook
                if (hooked[i]) dist[i] = (current[i].transform.position - (player.transform.position + new Vector3(0, 0.5f, 0))).normalized * 0.75f;

                //Delete when let go and pulled objects rigid body retains velocity
                if (!grips[i]) {
                    if (pull[i]) {
                        grabbed[i].GetComponent<BoxCollider>().enabled = true;
                        grabbed[i].GetComponent<Rigidbody>().AddForce((HandPos[i] - grabbed[i].transform.position).normalized, ForceMode.Impulse);
                    }
                    ClearHook(i);
                }

                //Rope max length
                if (Vector2.Distance(current[i].transform.position, player.transform.position) > grappleLength) ClearHook(i);

                //Objects pulled towards player
                if (pull[i]) {
                    current[i].transform.position = Vector3.MoveTowards(current[i].transform.position, HandPos[i], Time.deltaTime * fireSpd);
                    grabbed[i].transform.position = current[i].transform.position;
                    grabbed[i].transform.rotation = HandRot[i];
                    grabbed[i].GetComponent<BoxCollider>().enabled = false;
                }
            } else {
                //Velocity retained after hooked & released, using raycast collision detection
                if (Physics.Raycast(player.transform.position + Vector3.up, dist[i], out RaycastHit hit, 0.8f)) {
                    Vector3 scalar = new Vector3(Mathf.Abs(hit.normal.x), Mathf.Abs(hit.normal.y), Mathf.Abs(hit.normal.z));
                    scalar = Vector3.one - scalar;
                    dist[i] = Vector3.Scale(dist[i], scalar);
                }
                dist[i] = Vector3.MoveTowards(dist[i], Vector3.zero, tugSpd * Time.deltaTime);
                player.transform.position += dist[i];
                grabbed[i] = null;
            }

            //Rope is a line between the projectile and the hand position
            if (grips[i]) {
                HandVertex(i);
                ProjVertex(i);
                GenerateMesh(i);
            }
        }

        //Player is pulled and rotated towards most recent projectile that hooked
        if (hooked[0] ^ hooked[1]) {
            player.transform.position = Vector3.MoveTowards(player.transform.position, lastHit, pullSpd * Time.deltaTime);
        }

        //Player falls out of bounds
        if (player.transform.position.y < -20) player.transform.position = respawn;
    }

    //Spawn projectile hook with velocity
    private void GrapplingHook(int index)
    {
        current[index] = Instantiate(projectile, HandPos[index], HandRot[index]);
        current[index].GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * fireSpd, ForceMode.Impulse);
        current[index].GetComponent<Hooking>().Setup(this, index);
    }

    //When a hook hits a grapplable object, set it as the target position and reset the other hook
    public void GrappleHit(int index, float roof)
    {
        for (int i = 0; i < 2; i++) hooked[i] = (i == index);

        //Move the player to just before the wall, and adjust the line 
        Vector3 ray = ((player.transform.position - current[index].transform.position).normalized * 0.25f) - new Vector3(0, roof, 0);
        lastHit = reticle[index].transform.position + ray;
        current[index].transform.position = reticle[index].transform.position;
        GrappleLook(index);
    }

    //Rotate the player once to face the hook if it is not already visible
    private void GrappleLook(int index)
    {
        if (!reticle[index].GetComponent<Renderer>().isVisible) {
            Vector3 look = Vector3.RotateTowards(Vector3.up, new Vector3(lastHit.x, player.transform.position.y, lastHit.z), 180, 0.0f);
            player.transform.LookAt(look);
            player.transform.localEulerAngles = new Vector3(0, player.transform.localEulerAngles.y, 0);
        }
    }

    //Hook hits an object that is moveable
    public void GrapplePull(int index, GameObject grab)
    {
        for (int i = 0; i < 2; i++) pull[i] = (i == index);
        grabbed[index] = grab;
    }

    //Delete projectile and reset line
    public void ClearHook(int index)
    {
        Destroy(current[index]);
        ropes[index].Clear();
        grips[index] = false;
        hooked[index] = false;
        pull[index] = false;
    }

    //Sets the vertices relative to hands
    private void HandVertex(int i)
    {
        Vector3 up = Hands[i].up * ropeThickness;
        Vector3 right = Hands[i].right * ropeThickness;
        if (i == 0) {
            RVerts[0] = HandPos[i] + up;
            RVerts[1] = HandPos[i] + right;
            RVerts[2] = HandPos[i] - up;
            RVerts[3] = HandPos[i] - right;
        } else {
            LVerts[0] = HandPos[i] + up;
            LVerts[1] = HandPos[i] + right;
            LVerts[2] = HandPos[i] - up;
            LVerts[3] = HandPos[i] - right;
        }
    }

    //Sets the vertices relative to projectile
    private void ProjVertex(int i)
    {
        Vector3 up = current[i].transform.up * ropeThickness;
        Vector3 right = current[i].transform.right * ropeThickness;
        if (i == 0) {
            RVerts[4] = current[i].transform.position + up;
            RVerts[5] = current[i].transform.position + right;
            RVerts[6] = current[i].transform.position - up;
            RVerts[7] = current[i].transform.position - right;
        } else {
            LVerts[4] = current[i].transform.position + up;
            LVerts[5] = current[i].transform.position + right;
            LVerts[6] = current[i].transform.position - up;
            LVerts[7] = current[i].transform.position - right;
        }
    }

    //Set uvs to tile
    private void UVS(int i)
    {
        if (i == 0) {
            for (int j = 0; j < 4; j++) {
                float D = Vector3.Distance(current[i].transform.position, HandPos[i]) / 2;
                RUvs[j] = Vector2.zero;
                RUvs[j + 4] = new Vector2(D, 0);
            }
        } else {
            for (int j = 0; j < 4; j++) {
                float D = Vector3.Distance(current[i].transform.position, HandPos[i]) / 2;
                RUvs[j] = Vector2.zero;
                RUvs[j + 4] = new Vector2(D, 0);
            }
        }
    }

    //Creates a mesh using the given vertices
    private void GenerateMesh(int i)
    {
        if (ropes[i] == null) ropes[i] = new Mesh();
        ropes[i].Clear();
        UVS(i);
        if (i == 0) {
            ropes[i].vertices = RVerts;
            ropes[i].SetUVs(0, RUvs);
        } else {
            ropes[i].vertices = LVerts;
            ropes[i].SetUVs(0, LUvs);
        }
        ropes[i].triangles = Tris;
        ropes[i].RecalculateNormals();
        meshes[i].mesh = ropes[i];
    }

    //Inputs for both hands
    public void Fire(InputAction act, int hand)
    {
        float value = act.ReadValue<float>();
        if (value > triggerSensitivity && !grips[hand] && !held[hand]) {
            grips[hand] = true;
            held[hand] = true;
            GrapplingHook(hand);
        }
        if (value <= triggerSensitivity) {
            grips[hand] = false;
            held[hand] = false;
        }
    }
}