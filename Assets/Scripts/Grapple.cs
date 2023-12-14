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
    public Material[] reticolors;
    public GameObject projectile;
    public GameObject player;
    public GameObject[] controllers;
    public AudioSource[] reelIn;
    public AudioSource[] fireSFX;

    //Editor Values
    public float fireSpd;
    public float pullSpd;
    private float tugSpd;
    public float triggerSensitivity;
    public float grappleLength;
    public float ropeThickness;
    public float gravityStrength;
    public float slidiness;

    //States
    private bool[] grips;
    private bool[] hooked;
    private bool[] held;
    private bool[] pull;
    private bool[] canLatch;
    private float[] limitedDistances;
    private string[] grabTag;
    private bool usingGravity;

    //Privates
    private Vector3 respawn;
    private GameObject[] grabbed;
    private Vector3[] dist;
    private GameObject[] current;
    private Vector3 lastHit;
    private float gravVelocity;

    //Mesh Data
    private Mesh[] ropes;
    private Vector3[][] Verts;
    private Vector2[][] Uvs;
    private readonly int[] Tris = new int[] { 0, 4, 1, 4, 5, 1, 1, 5, 2, 5, 6, 2, 2, 6, 3, 6, 7, 3, 3, 7, 0, 7, 4, 0, 0, 1, 3, 1, 2, 3 };

    void Awake()
    {
        usingGravity = true;
        HandPos = new Vector3[2];
        HandRot = new Quaternion[2];

        held = new bool[2];
        pull = new bool[2];
        grips = new bool[2];
        hooked = new bool[2];
        canLatch = new bool[2];
        limitedDistances = new float[2];
        grabTag = new string[2];
        dist = new Vector3[2];
        current = new GameObject[2];
        grabbed = new GameObject[2];

        ropes = new Mesh[2];
        Verts = new Vector3[2][];
        Uvs = new Vector2[2][];
        for (int i = 0; i < 2; i++) {
            Verts[i] = new Vector3[8];
            Uvs[i] = new Vector2[8];
        }

        triggerSensitivity = Mathf.Clamp01(triggerSensitivity);
        tugSpd = pullSpd / 10;

        respawn = player.transform.position;
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
                    grabTag[i] = hit.collider.tag;
                    if (hit.collider.CompareTag("Grapple") || hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Ceiling") || hit.collider.CompareTag("Grabbable")) {
                        if (!canLatch[i]) {
                            reticle[i].GetComponent<Renderer>().material = reticolors[0];
                            canLatch[i] = true;
                        }
                    } else {
                        if (canLatch[i]) {
                            reticle[i].GetComponent<Renderer>().material = reticolors[1];
                            canLatch[i] = false;
                        }
                    }
                    reticle[i].SetActive(true);
                    reticle[i].transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
                    limitedDistances[i] = hit.distance + 2;
                } else {
                    reticle[i].SetActive(false);
                }
            }
        }

        //Check both hands for grappling hooks
        for (int i = 0; i < 2; i++) {
            if (current[i] != null) {
                //Distance between player and hook
                if (hooked[i]) dist[i] = (current[i].transform.position - (player.transform.position + new Vector3(0, 0.667f, 0))).normalized * slidiness;

                //Delete when let go and pulled objects rigid body retains velocity
                if (!grips[i]) {
                    if (pull[i]) grabbed[i].GetComponent<Object>().Released(HandPos[i] - grabbed[i].transform.position);

                    //Cap the upward velocity retension
                    if (dist[i].y > 0.7f) dist[i].y = 0.625f;

                    ClearHook(i);
                }

                //Rope max length, either from distance or from raycast limit
                float distance = Vector3.Distance(current[i].transform.position, HandPos[i]);
                if (distance > grappleLength || distance > limitedDistances[i]) ClearHook(i);

                //Rope min length when returning
                if (current[i].GetComponent<Hooking>().returning) {
                    if (Vector3.Distance(current[i].transform.position, HandPos[i]) < 0.75f) ClearHook(i);
                }

                //Objects pulled towards player
                if (pull[i]) {
                    current[i].transform.position = Vector3.MoveTowards(current[i].transform.position, HandPos[i], Time.deltaTime * fireSpd);
                    grabbed[i].transform.SetPositionAndRotation(current[i].transform.position, HandRot[i]);
                }
            } else {
                //Velocity retained after hooked & released, using raycast collision detection from center & from ground
                if (Physics.Raycast(player.transform.position + Vector3.up, dist[i].normalized, out RaycastHit hit, 0.8f)) {
                    Vector3 scalar = new Vector3(Mathf.Abs(hit.normal.x), Mathf.Abs(hit.normal.y), Mathf.Abs(hit.normal.z));
                    scalar = Vector3.one - scalar;
                    dist[i] = Vector3.Scale(dist[i], scalar);
                }
                if (Physics.Raycast(player.transform.position, Vector3.down, 0.2f)) {
                    Vector3 scalar = new Vector3(1, 0, 1);
                    dist[i] = Vector3.Scale(dist[i], scalar);
                }
                dist[i] = Vector3.MoveTowards(dist[i], Vector3.zero, tugSpd * Time.deltaTime);
                if (dist[i].magnitude > 0.6f) player.transform.position += (dist[i].normalized * 0.6f);
                else player.transform.position += dist[i];

                grabbed[i] = null;
                reelIn[i].volume = 0;
            }

            //Rope is a line between the projectile and the hand position
            if (grips[i]) {
                HandVertex(i);
                ProjVertex(i);
                GenerateMesh(i);
            }
        }

        //Update Gravity
        usingGravity = !(hooked[0] || hooked[1]);

        //Player is pulled and rotated towards most recent projectile that hooked
        if (hooked[0] ^ hooked[1]) {
            int i = hooked[0] ? 0 : 1;
            //Raycast to prevent clipping into ground
            Vector3 downward = dist[i].normalized;
            downward = new Vector3(downward.x, downward.y - 0.167f, downward.z);
            if (Physics.Raycast(player.transform.position, downward, 0.2f)) {
                Vector3 slide = new Vector3(lastHit.x, player.transform.position.y, lastHit.z);
                player.transform.position = Vector3.MoveTowards(player.transform.position, slide, pullSpd * Time.deltaTime);
            } else {
                player.transform.position = Vector3.MoveTowards(player.transform.position, lastHit, pullSpd * Time.deltaTime);
            }
            reelIn[i].volume = 0.5f;
        }

        //Player falls out of bounds
        if (player.transform.position.y < -20) player.transform.position = respawn;
    }

    //Player gravity
    private void FixedUpdate()
    {
        Vector3 offset = new Vector3(player.transform.position.x, player.transform.position.y + 0.35f, player.transform.position.z);
        if (usingGravity) {
            if (Physics.Raycast(offset, Vector3.down, out RaycastHit ground, 1.5f)) {
                gravVelocity = 0;
                float Y = ground.point.y + 0.05f;
                if (player.transform.position.y - Y > 0.05f) {
                    Vector3 change = new Vector3(0, (player.transform.position.y - Y) / 2, 0);
                    player.transform.position -= change;
                }
            } else {
                gravVelocity = Mathf.Max(gravVelocity - (Time.deltaTime * gravityStrength), -3);
                Vector3 downward = new Vector3(0, gravVelocity, 0);
                player.transform.position += downward;
                if (Physics.Raycast(offset, Vector3.down, 1.5f)) gravVelocity = 0;
            }
        } else {
            gravVelocity = 0;
        }
    }

    //Spawn projectile hook with velocity
    private void GrapplingHook(int index)
    {
        current[index] = Instantiate(projectile, HandPos[index], HandRot[index]);
        current[index].GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * fireSpd, ForceMode.Impulse);
        current[index].GetComponent<Hooking>().Setup(this, index);
        controllers[index].SetActive(false);
    }

    //When a hook hits a grapplable object, set it as the target position and reset the other hook
    public void GrappleHit(int index)
    {
        for (int i = 0; i < 2; i++) {
            hooked[i] = (i == index);
            dist[i] = Vector3.zero;
        }

        //Use the raycast to determine roof offset
        Vector3 offset = Vector3.zero;
        if (grabTag[index].Equals("Grapple")) offset = new Vector3(0, 1, 0);
        if (grabTag[index].Equals("Ceiling")) offset = new Vector3(0, 1.6f, 0);
        if (grabTag[index].Equals("Ground")) offset = new Vector3(0, -0.2f, 0);

        //Move the player to just before the wall, and adjust the line 
        Vector3 ray = ((player.transform.position - current[index].transform.position).normalized * 0.25f) - offset;
        lastHit = reticle[index].transform.position + ray;
        current[index].transform.position = reticle[index].transform.position;
    }

    //Hook hits an object that is moveable
    public void GrapplePull(int index, GameObject grab)
    {
        for (int i = 0; i < 2; i++) pull[i] = (i == index);
        grab.GetComponent<Object>().Grabbed();
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
        controllers[index].SetActive(true);
    }

    //Sets the vertices relative to hands
    private void HandVertex(int i)
    {
        Vector3 up = Hands[i].up * ropeThickness;
        Vector3 right = Hands[i].right * ropeThickness;
        Verts[i][0] = HandPos[i] + up;
        Verts[i][1] = HandPos[i] + right;
        Verts[i][2] = HandPos[i] - up;
        Verts[i][3] = HandPos[i] - right;
    }

    //Sets the vertices relative to projectile
    private void ProjVertex(int i)
    {
        Vector3 up = current[i].transform.up * ropeThickness;
        Vector3 right = current[i].transform.right * ropeThickness;
        Verts[i][4] = current[i].transform.position + up;
        Verts[i][5] = current[i].transform.position + right;
        Verts[i][6] = current[i].transform.position - up;
        Verts[i][7] = current[i].transform.position - right;
    }

    //Set uvs to tile
    private void UVS(int i)
    {
        for (int j = 0; j < 4; j++) {
            float D = Vector3.Distance(current[i].transform.position, HandPos[i]) / 2;
            Uvs[i][j] = Vector2.zero;
            Uvs[i][j + 4] = new Vector2(D, 0);
        }
    }

    //Creates a mesh using the given vertices
    private void GenerateMesh(int i)
    {
        if (ropes[i] == null) ropes[i] = new Mesh();
        ropes[i].Clear();
        UVS(i);
        ropes[i].vertices = Verts[i];
        ropes[i].SetUVs(0, Uvs[i]);
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
            fireSFX[hand].Play();
            GrapplingHook(hand);
        }
        if (value <= triggerSensitivity) {
            grips[hand] = false;
            held[hand] = false;
        }
    }
}