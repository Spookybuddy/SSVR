using UnityEngine;

public class Steering : MonoBehaviour
{
    public Transform[] Hands;
    public Transform player;
    public Transform[] nodes;
    public Rigidbody rigid;
    public float range;
    public Vector3 pivot;

    void Update()
    {
        //Only update if player is close enough
        if (Vector3.Distance(player.position, transform.position) < 4) {
            for (int h = 0; h < Hands.Length; h++) {
                for (int i = 0; i < nodes.Length; i++) {
                    Vector3 difference = (Hands[h].position - nodes[i].position);
                    Vector3 normal = difference.normalized;
                    Vector3 right = nodes[i].right;
                    if (Vector3.Distance(Hands[h].position, nodes[i].position) < range) {
                        //Check if within ranges
                        if (Mathf.Abs(normal.x) < Mathf.Abs(right.x) || Mathf.Abs(normal.y) < Mathf.Abs(right.y)) {
                            float force = 0.05f / difference.magnitude;
                            if (Mathf.Abs(right.x) > Mathf.Abs(right.y)) rigid.AddTorque(force * Mathf.Sign(normal.x * right.x) * pivot, ForceMode.Impulse);
                            else rigid.AddTorque(force * Mathf.Sign(normal.y * right.y) * pivot, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }
}