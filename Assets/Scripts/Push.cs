using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : MonoBehaviour
{
    private Vector3 zeroed;
    private Vector3 final;
    public Vector3 direction;
    public Transform[] hands;
    private bool press;
    public float value;

    private void Start()
    {
        zeroed = transform.position;
        final = zeroed + direction;
    }

    private void FixedUpdate()
    {
        press = false;
        for (int i = 0; i < hands.Length; i++) {
            if (Vector3.Distance(hands[i].position, transform.position) < 0.5f) {
                press = true;
                transform.position = Vector3.Lerp(zeroed, final, value);
            }
        }

        if (press) {
            value = Mathf.Clamp01(value + Time.deltaTime);
        } else {
            value = Mathf.Clamp01(value - Time.deltaTime);
            transform.position = Vector3.Lerp(zeroed, final, value);
        }
    }
}