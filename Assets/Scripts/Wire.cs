using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform target;
    public LineRenderer lineForNow;
    public float maxDistance;
    private Vector3 start;
    private Rigidbody targetRig;

    private void Start()
    {
        start = transform.position;
        targetRig = target.GetComponent<Rigidbody>();
        lineForNow.SetPosition(0, start);
    }

    private void FixedUpdate()
    {
        //Too far away
        if (Vector3.Distance(start, target.position) > maxDistance) {
            target.parent = transform;
            target.position = start;
        }

        //Renable gravity once far enough away
        if (Vector3.Distance(start, target.position) > 0.1f) {
            targetRig.useGravity = true;
        }

        //Velocity?
        if (targetRig.velocity.magnitude > 0.1f) {
            targetRig.useGravity = false;
            targetRig.velocity = Vector3.zero;
            target.position = start;
        }

        //Line renderer. Looks bad
        lineForNow.SetPosition(1, target.position);
    }
}