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
        } else { player.ClearHook(index); }
    }
}