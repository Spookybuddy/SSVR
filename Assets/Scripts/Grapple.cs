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
    public LineRenderer rope;
    public GameObject projectile;
    public GameObject player;

    //Editor Values
    public float fireSpd;
    public float pullSpd;
    public float triggerSensitivity;
    public float decayRate;

    //States
    private bool fired;
    private bool hooked;

    //Privates
    private Vector3 dist;
    private GameObject current;
    private readonly Vector3[] zeroed = new Vector3[] {Vector3.zero, Vector3.zero};

    void Awake()
    {
        triggerSensitivity = Mathf.Clamp01(triggerSensitivity);
    }

    private void Update()
    {
        //Read inputs
        FireRH(gripActionR.action);

        //Track hand position
        RHpos = RH.transform.position;
        RHrot = RH.transform.rotation;

        //Destroy projectile if button is let go
        if (!fired && current != null) ClearHook();

        //Rope is a line between the projectile and the hand position
        if (fired || hooked) {
            rope.SetPosition(0, RHpos);
            rope.SetPosition(1, current.transform.position);
        }

        //Player is pulled towards the projectile when it hooks. Add swing physics later
        if (hooked) {
            player.transform.position = Vector3.MoveTowards(player.transform.position, current.transform.position, pullSpd * Time.deltaTime);
        } else {
            /*
            if (current != null) {
                dist = (player.transform.position - current.transform.position).normalized;
            } else {
                player.transform.position += dist;
                dist = Vector3.MoveTowards(dist, Vector3.zero, Time.deltaTime * 5);
            }
            */
        }
    }

    //Spawn projectile hook with velocity
    private void GrapplingHook()
    {
        current = Instantiate(projectile, RHpos, RHrot) as GameObject;
        current.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * fireSpd, ForceMode.Impulse);
        current.GetComponent<Hooking>().player = this;
    }

    public void GrappleHit() { hooked = true; }

    //Delete projectile and reset line
    public void ClearHook()
    {
        Destroy(current);
        rope.SetPositions(zeroed);
        fired = false;
        hooked = false;
    }

    //Inputs
    public void FireRH(InputAction act)
    {
        float value = act.ReadValue<float>();
        if (value > triggerSensitivity && !fired) {
            fired = true;
            GrapplingHook();
        }
        if (value <= triggerSensitivity) fired = false;
    }
}