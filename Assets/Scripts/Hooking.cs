using UnityEngine;

public class Hooking : MonoBehaviour
{
    private Grapple player;
    private int index;
    public bool returning;
    public Rigidbody rigid;

    public GameObject[] soundClip;

    //Passed from hand on fire
    public void Setup(Grapple G, int N)
    {
        player = G;
        index = N;
    }

    //Enters a collider
    void OnTriggerEnter(Collider tag)
    {
        //Do not hook if ricochet
        if (returning) return;

        //Check tags to perform specific actions
        switch (tag.tag) {
            case "Grapple":
            case "Ceiling":
            case "Ground":
                Instantiate(soundClip[0], transform.position, Quaternion.identity);
                Attach();
                return;
            case "Grabbable":
                Instantiate(soundClip[0], transform.position, Quaternion.identity);
                rigid.velocity = Vector3.zero;
                player.GrapplePull(index, tag.gameObject);
                return;
            case "Start":
                Instantiate(soundClip[1], transform.position, Quaternion.identity);
                GameObject.FindWithTag("GameController").GetComponent<GameManager>().LoadScene();
                player.ClearHook(index);
                return;
            case "MainMenu":
                Instantiate(soundClip[2], transform.position, Quaternion.identity);
                GameObject.FindWithTag("GameController").GetComponent<GameManager>().OpenMenu(0);
                player.ClearHook(index);
                return;
            case "Credits":
                Instantiate(soundClip[1], transform.position, Quaternion.identity);
                GameObject.FindWithTag("GameController").GetComponent<GameManager>().OpenMenu(1);
                player.ClearHook(index);
                return;
            case "Fridge":
                Instantiate(soundClip[3], transform.position, Quaternion.identity);
                tag.GetComponent<Fridge>().Play();
                break;
            case "Mute":
                ReturnCheck();
                return;
        }
        if (!returning) Instantiate(soundClip[4], transform.position, Quaternion.identity);
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