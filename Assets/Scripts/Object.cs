using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Object : MonoBehaviour
{
    private Rigidbody rigid;
    private Vector3 spawn;
    private Quaternion rotation;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        spawn = transform.position;
        rotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -20) Respawn();
    }

    private void Respawn()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        transform.position = spawn;
        transform.rotation = rotation;
    }
}