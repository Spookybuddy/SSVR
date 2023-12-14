using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    private bool state;
    private bool current;
    private bool moving;
    public AudioSource[] openCloseSFX;
    private Transform player;
    public GameObject wall;
    public float openSpd;
    public Transform[] doorParts;
    private Vector3[] begins;
    public Vector3[] ends;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        begins = new Vector3[doorParts.Length];
        for (int i = 0; i < doorParts.Length; i++) {
            begins[i] = doorParts[i].localPosition;
            ends[i] += doorParts[i].localPosition;
        }
    }

    //Move doors to state only when called
    void Update()
    {
        if (moving) {
            moving = false;
            for (int i = 0; i < doorParts.Length; i++) {
                doorParts[i].localPosition = Vector3.MoveTowards(doorParts[i].localPosition, state ? ends[i] : begins[i], Time.deltaTime * openSpd * 4);
                if (Vector3.Distance(doorParts[i].localPosition, state ? ends[i] : begins[i]) > 0.1f) moving = true;
            }
        }
    }

    //Resort to player distance to open / close
    void FixedUpdate()
    {
        state = (Vector3.Distance(transform.position, player.position) < 4);
        if (state != current) {
            moving = true;
            StartCoroutine(DoorMove());
            current = state;
        }
    }

    //Open door spd
    private IEnumerator DoorMove()
    {
        openCloseSFX[state ? 1 : 0].Play();
        yield return new WaitForSeconds(openSpd);
        wall.SetActive(!state);
    }
}