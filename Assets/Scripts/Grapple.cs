using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grapple : MonoBehaviour
{
    //Hand tracking
    public Vector3 RHpos;
    public Quaternion RHrot;
    private Vector3 LHpos;
    private Quaternion LHrot;

    //Prefabs
    public LineRenderer rope;
    public GameObject projectile;
    public GameObject player;
    public float pullSpd;

    //States
    public bool fired;
    public bool hooked;

    //Privates
    private GameObject current;
    private readonly Vector3[] zeroed = new Vector3[] {Vector3.zero, Vector3.zero};

    private void Update()
    {
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
        }
    }

    //Spawn projectile hook with velocity
    private void GrapplingHook()
    {
        current = Instantiate(projectile, RHpos, RHrot) as GameObject;
        current.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 5, ForceMode.Impulse);
    }

    //Delete projectile and reset line
    private void ClearHook()
    {
        Destroy(current);
        rope.SetPositions(zeroed);
        fired = false;
        hooked = false;
    }

    //Inputs
    public void TrackRHpos(InputAction.CallbackContext ctx) { RHpos = ctx.ReadValue<Vector3>(); }
    public void TrackRHrot(InputAction.CallbackContext ctx) { RHrot = ctx.ReadValue<Quaternion>(); }
    public void FireRH(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !fired) {
            fired = true;
            GrapplingHook();
        }
        if (ctx.canceled) fired = false;
    }
}