using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform target;
    private Transform replacement;
    private Transform parental;
    public float maxDistance;
    private Vector3 start;
    private Rigidbody targetRig;
    private bool retract;

    private void Start()
    {
        parental = transform.parent;
        start = transform.position;
        targetRig = target.GetComponent<Rigidbody>();
        retract = false;
    }

    private void FixedUpdate()
    {
        //Too far away
        if (Vector3.Distance(start, target.position) > maxDistance) {
            GetComponents();
            ResetPhysics();
            retract = true;
            parental.GetComponent<PuzzleSystem>().Restore(GetComponent<Puzzle>());
        }

        //Renable gravity once far enough away
        if (!targetRig.useGravity) {
            if (Vector3.Distance(start, target.position) > 0.1f) targetRig.useGravity = true;
        }

        //Velocity?
        if (targetRig.velocity.magnitude > 0.1f) {
            ResetPhysics();
            retract = true;
        }

        //Change from instant teleport to pull back for coolness sake
        if (retract) {
            target.transform.position = Vector3.MoveTowards(target.position, start, maxDistance * Time.deltaTime * 2);
            if (Vector3.Distance(start, target.position) < 0.1f) {
                retract = false;
                target.transform.position = start;
            }
        }
    }

    //Destroys and replaces the wire grab so the player lets go
    private void GetComponents()
    {
        replacement = Instantiate(target, Vector3.zero, Quaternion.identity);
        targetRig = replacement.GetComponent<Rigidbody>();
        replacement.name = target.name;
        replacement.parent = parental;
        Destroy(target.gameObject);
        target = replacement;
        replacement = null;
    }

    //Reset physics to 0
    private void ResetPhysics()
    {
        targetRig.isKinematic = false;
        targetRig.useGravity = false;
        targetRig.velocity = Vector3.zero;
        targetRig.angularVelocity = Vector3.zero;
        target.rotation = Quaternion.identity;
    }

    //Return to goal
    public void Freeze(Vector3 point)
    {
        start = point;
        GetComponents();
        ResetPhysics();
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Puzzle>().completed = true;
        target.transform.position = start;
        target.transform.rotation = Quaternion.identity;
        parental.GetComponent<PuzzleSystem>().Restore(GetComponent<Puzzle>());
    }
}