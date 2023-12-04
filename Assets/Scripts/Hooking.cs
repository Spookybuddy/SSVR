using UnityEngine;

public class Hooking : MonoBehaviour
{
    private Grapple player;
    private int index;
    public bool returning;
    public Rigidbody rigid;

    public void Setup(Grapple G, int N)
    {
        player = G;
        index = N;
    }

    void OnTriggerEnter(Collider other)
    {
        //Riccocheted grapple ignores colliders
        if (returning && !other.CompareTag("Door")) {
            player.ClearHook(index);
            return;
        }

        //Can only grab onto objects tagged so
        if (other.CompareTag("Grapple")) {
            //Generic grapple
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, 1);
        } else if (other.CompareTag("Ceiling")) {
            //Vertical offset is greater on roof
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, 1.5f);
        } else if (other.CompareTag("Grabbable")) {
            //Pull towards player
            rigid.velocity = Vector3.zero;
            player.GrapplePull(index, other.gameObject);
        } else if (other.CompareTag("Ground")) {
            //Additional vertical offset on floor
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, -0.2f);
        } else {
            //Ungrabbable
            if (!returning) {
                rigid.velocity = -rigid.velocity;
                returning = true;
            }
        }
    }
}