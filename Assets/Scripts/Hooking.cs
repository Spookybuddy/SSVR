using UnityEngine;

public class Hooking : MonoBehaviour
{
    private Grapple player;
    private int index;
    private bool returning;
    public Rigidbody rigid;

    public void Setup(Grapple G, int N)
    {
        player = G;
        index = N;
    }

    void OnTriggerEnter(Collider other)
    {
        //Can only grab onto objects tagged so
        if (other.CompareTag("Grapple")) {
            if (returning) player.ClearHook(index);
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, 1);
        } else if (other.CompareTag("Ceiling")) {
            //Vertical offset is greater on roof
            if (returning) player.ClearHook(index);
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, 1.5f);
        } else if (other.CompareTag("Grabbable")) {
            if (returning) player.ClearHook(index);
            rigid.velocity = Vector3.zero;
            player.GrapplePull(index, other.gameObject);
        } else if (other.CompareTag("Door")) {
            if (!returning) {
                rigid.velocity = -rigid.velocity;
                returning = true;
            }
        } else {
            player.ClearHook(index);
        }
    }
}