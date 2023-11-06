using UnityEngine;

public class Hooking : MonoBehaviour
{
    public Grapple player;
    public Rigidbody rigid;
    void OnTriggerEnter(Collider other)
    {
        //Can only grab onto objects tagged so
        if (other.CompareTag("Grapple")) {
            rigid.velocity = Vector3.zero;
            player.GrappleHit();
        } else if (other.CompareTag("Player")) {
            //Go through player
            return;
        } else { player.ClearHook(); }
    }
}