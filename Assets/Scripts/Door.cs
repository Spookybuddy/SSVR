using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private bool state;
    private bool status;
    public float openSpd;
    public Transform left;
    private Vector3 Lstart;
    private Vector3 Lend;
    public Transform right;
    private Vector3 Rstart;
    private Vector3 Rend;

    private void Start()
    {
        Lstart = left.position;
        Lend = left.position + (left.forward * -2);
        Rstart = right.position;
        Rend = right.position + (right.forward * 2);
    }

    void Update()
    {
        if (state != status) {
            left.position = Vector3.MoveTowards(left.position, state ? Lend : Lstart, Time.deltaTime * openSpd * 3);
            right.position = Vector3.MoveTowards(right.position, state ? Rend : Rstart, Time.deltaTime * openSpd * 3);
        }
    }

    //Player enters bounds
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            state = true;
            StartCoroutine(DoorMove());
        }
    }

    //Player exits bounds
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) {
            state = false;
            StartCoroutine(DoorMove());
        }
    }

    //UI open function
    public void DoorButton()
    {
        state = !state;
        DoorMove();
    }

    //Open door spd
    private IEnumerator DoorMove()
    {
        yield return new WaitForSeconds(openSpd);
        if (state != status) status = state;
    }
}