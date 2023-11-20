using UnityEngine;

public class Hooking : MonoBehaviour
{
    private Grapple player;
    private int index;
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
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, 1);
        } else if (other.CompareTag("Ceiling")) {
            //Vertical offset is greater on roof
            rigid.velocity = Vector3.zero;
            player.GrappleHit(index, 1.5f);
        } else if (other.CompareTag("Grabbable")) {
            rigid.velocity = Vector3.zero;
            player.GrapplePull(index, other.gameObject);
        } else if (other.CompareTag("Door")) {
            return;
        } else {
            player.ClearHook(index);
        }
    }
}