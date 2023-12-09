using System.Collections;
using UnityEngine;

public class Fridge : MonoBehaviour
{
    public Animator animate;
    public float resetTime;
    private bool triggered;

    public void Play()
    {
        if (!triggered) {
            animate.SetBool("Shot", true);
            StartCoroutine(Close());
        }
    }

    private IEnumerator Close()
    {
        triggered = true;
        yield return new WaitForSeconds(resetTime);
        animate.SetBool("Shot", false);
        triggered = false;
    }
}