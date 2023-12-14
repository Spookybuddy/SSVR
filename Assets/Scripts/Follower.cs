using UnityEngine;

public class Follower : MonoBehaviour
{
    public AudioSource music;
    public Transform player;
    public float affordance;
    public float followSpd;

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) > affordance) {
            transform.position = Vector3.MoveTowards(transform.position, player.position, Time.deltaTime * followSpd);
        }
    }
}