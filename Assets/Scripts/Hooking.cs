using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hooking : MonoBehaviour
{
    public Grapple player;
    public Rigidbody rigid;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Grapple>();
    }

    void OnTriggerEnter(Collider other)
    {
        rigid.velocity = Vector3.zero;
        player.hooked = true;
    }
}