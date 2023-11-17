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
            player.GrappleHit(index);
        }
        else if (other.CompareTag("Grabbable")) {
            rigid.velocity = Vector3.zero;
            player.GrapplePull(index, other.gameObject);
        } else {
            player.ClearHook(index);
        }
    }
}