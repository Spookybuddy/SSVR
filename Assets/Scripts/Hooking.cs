using UnityEngine;

public class Hooking : MonoBehaviour
{
    private Grapple player;
    private int index;
    public bool returning;
    public Rigidbody rigid;

    //Passed from hand on fire
    public void Setup(Grapple G, int N)
    {
        player = G;
        index = N;
    }

    //Enters a collider
    void OnTriggerEnter(Collider tag)
    {
        switch (tag.tag) {
            case "Grapple":
            case "Ceiling":
            case "Ground":
                Attach();
                return;
            case "Grabbable":
                rigid.velocity = Vector3.zero;
                player.GrapplePull(index, tag.gameObject);
                return;
            case "Start":
                GameObject.FindWithTag("GameController").GetComponent<GameManager>().LoadScene();
                player.ClearHook(index);
                return;
            case "MainMenu":
                GameObject.FindWithTag("GameController").GetComponent<GameManager>().OpenMenu(0);
                player.ClearHook(index);
                return;
            case "Credits":
                GameObject.FindWithTag("GameController").GetComponent<GameManager>().OpenMenu(1);
                player.ClearHook(index);
                return;
            case "Fridge":
                tag.GetComponent<Fridge>().Play();
                ReturnCheck();
                break;
        }
        ReturnCheck();
    }

    //Surface hit that can pull player
    private void Attach()
    {
        rigid.velocity = Vector3.zero;
        player.GrappleHit(index);
    }

    //Bounce off collider
    private void ReturnCheck()
    {
        if (!returning) {
            rigid.velocity = -rigid.velocity;
            returning = true;
        }
    }
}