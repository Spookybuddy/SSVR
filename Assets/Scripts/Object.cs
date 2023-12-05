using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Object : MonoBehaviour
{
    private Rigidbody rigid;
    private Vector3 spawn;
    private Quaternion rotation;
    private Collider template;
    private float limit = 8;
    private float lower = 0.1f;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        spawn = transform.position;
        rotation = transform.rotation;
        template = transform.GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -20) Respawn();
        if (rigid.velocity.magnitude > Mathf.Pow(limit, 4)) rigid.velocity = rigid.velocity.normalized * (limit * limit);
    }

    //Return to orign after falling out of bounds
    private void Respawn()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        transform.position = spawn;
        transform.rotation = rotation;
        template.enabled = true;
    }

    //called when grabbed to shut off rigidbody & collider
    public void Grabbed()
    {
        template.enabled = false;
        rigid.useGravity = false;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    //Called when released to turn back on rigidbody, add force, and return collider
    public void Released(Vector3 force)
    {
        template.enabled = true;
        if (force.magnitude < lower) rigid.AddForce(force * (limit * limit), ForceMode.Impulse);
        if (force.magnitude > limit) force = force.normalized;
        rigid.AddForce(force * limit, ForceMode.Impulse);
        rigid.useGravity = true;
    }
}