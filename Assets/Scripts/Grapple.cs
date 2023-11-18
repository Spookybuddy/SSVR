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
    public LineRenderer[] rope;
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
                if (hooked[i]) dist[i] = (current[i].transform.position - player.transform.position).normalized * 0.75f;

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
                //Velocity retained after hooked & released. Add in raycast collision detection
                if (Physics.Raycast(player.transform.position, dist[i], out RaycastHit hit, 5, 2, QueryTriggerInteraction.Ignore)) {
                    dist[i] = Vector3.Reflect(dist[i], hit.normal);
                    Debug.Log("Collide");
                }
                dist[i] = Vector3.MoveTowards(dist[i], Vector3.zero, tugSpd * Time.deltaTime);
                player.transform.position += dist[i];
                grabbed[i] = null;
            }

            //Rope is a line between the projectile and the hand position
            if (grips[i]) {
                rope[i].SetPosition(0, HandPos[i]);
                rope[i].SetPosition(1, current[i].transform.position);
            }
        }

        //Player is pulled towards most recent projectile that hooked
        if (hooked[0] ^ hooked[1]) {
            player.transform.position = Vector3.MoveTowards(player.transform.position, lastHit, pullSpd * Time.deltaTime);
            if (Vector3.Distance(player.transform.position, lastHit) < severanceRange) ClearHook(hooked[0] ? 0 : 1);
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
    public void GrappleHit(int index)
    {
        for (int i = 0; i < 2; i++) hooked[i] = (i == index);
        lastHit = current[index].transform.position;
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
        rope[index].SetPositions(zeroed);
        grips[index] = false;
        hooked[index] = false;
        pull[index] = false;
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