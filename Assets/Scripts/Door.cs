using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    private bool state;
    private bool called;
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
            begins[i] = doorParts[i].position;
            ends[i] += doorParts[i].position;
        }
    }

    //Move doors to state only when called
    void Update()
    {
        if (called) {
            for (int i = 0; i < doorParts.Length; i++) {
                doorParts[i].position = Vector3.MoveTowards(doorParts[i].position, state ? ends[i] : begins[i], Time.deltaTime * openSpd * 4);
            }
        }
    }

    //Resort to player distance to open / close
    void FixedUpdate()
    {
        state = (Vector3.Distance(transform.position, player.position) < 4);
        if (!called) {
            StartCoroutine(DoorMove());
            called = true;
        }
    }

    //UI open function
    public void DoorButton()
    {
        state = !state;
        called = true;
        StartCoroutine(DoorMove());
    }

    //Open door spd
    private IEnumerator DoorMove()
    {
        yield return new WaitForSeconds(openSpd);
        called = false;
        wall.SetActive(!state);
    }
}