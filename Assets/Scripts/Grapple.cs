using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grapple : MonoBehaviour
{
    //Hand tracking
    public GameObject RH;
    private Vector3 RHpos;
    private Quaternion RHrot;
    public GameObject LH;
    private Vector3 LHpos;
    private Quaternion LHrot;

    //Prefabs
    public InputActionReference gripActionR;
    public InputActionReference gripActionL;
    public LineRenderer[] rope;
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
    private bool[] hands;
    private bool[] hooked;
    private bool[] held;

    //Privates
    private Vector3[] dist;
    private GameObject[] current;
    private Vector3 lastHit;
    private readonly Vector3[] zeroed = new Vector3[] {Vector3.zero, Vector3.zero};

    void Awake()
    {
        held = new bool[2];
        hands = new bool[2];
        hooked = new bool[2];
        dist = new Vector3[2];
        current = new GameObject[2];

        dist = zeroed;
        triggerSensitivity = Mathf.Clamp01(triggerSensitivity);
        tugSpd = pullSpd / 5;
    }

    private void Update()
    {
        //Read inputs
        Fire(gripActionR.action, 0);
        Fire(gripActionL.action, 1);

        //Track hand position
        RHpos = RH.transform.position;
        RHrot = RH.transform.rotation;
        LHpos = LH.transform.position;
        LHrot = LH.transform.rotation;

        //Check both hands for grappling hooks
        for (int i = 0; i < 2; i++) {
            //Velocity retension, button released, and grapple length limit
            if (current[i] != null) {
                if (hooked[i]) dist[i] = (current[i].transform.position - player.transform.position).normalized / 2;

                if (!hands[i]) ClearHook(i);

                if (Vector2.Distance(current[i].transform.position, player.transform.position) > grappleLength) ClearHook(i);
            } else {
                //Velocity retained after hooked & released. Add in raycast collision detection. Maybe even convert the grapple into raycast
                dist[i] = Vector3.MoveTowards(dist[i], Vector3.zero, tugSpd * Time.deltaTime);
                player.transform.position += dist[i];
            }

            //Rope is a line between the projectile and the hand position
            if (hands[i]) {
                rope[i].SetPosition(0, i == 0 ? RHpos : LHpos);
                rope[i].SetPosition(1, current[i].transform.position);
            }
        }

        //Player is pulled towards most recent projectile that hooked
        if (hooked[0] ^ hooked[1]) {
            player.transform.position = Vector3.MoveTowards(player.transform.position, lastHit, pullSpd * Time.deltaTime);
            if (Vector3.Distance(player.transform.position, lastHit) < severanceRange) ClearHook(hooked[0] ? 0 : 1);
        }
    }

    //Spawn projectile hook with velocity
    private void GrapplingHook(int index, Vector3 pos, Quaternion rot)
    {
        current[index] = Instantiate(projectile, pos, rot) as GameObject;
        current[index].GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * fireSpd, ForceMode.Impulse);
        current[index].GetComponent<Hooking>().Setup(this, index);
    }

    //When a hook hits a grapplable object, set it as the target position and reset the other hook
    public void GrappleHit(int index)
    {
        hooked[0] = false;
        hooked[1] = false;
        hooked[index] = true;
        lastHit = current[index].transform.position;
    }

    //Delete projectile and reset line
    public void ClearHook(int index)
    {
        Destroy(current[index]);
        rope[index].SetPositions(zeroed);
        hands[index] = false;
        hooked[index] = false;
    }

    //Inputs for both hands
    public void Fire(InputAction act, int hand)
    {
        float value = act.ReadValue<float>();
        if (value > triggerSensitivity && !hands[hand] && !held[hand]) {
            hands[hand] = true;
            held[hand] = true;
            if (hand == 0) GrapplingHook(0, RHpos, RHrot);
            else GrapplingHook(1, LHpos, LHrot);
        }
        if (value <= triggerSensitivity) {
            hands[hand] = false;
            held[hand] = false;
        }
    }
}