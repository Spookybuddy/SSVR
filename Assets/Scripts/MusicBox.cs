using System.Collections;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    public AudioSource sound;

    void Start()
    {
        sound.Play();
        StartCoroutine(End(sound.clip.length + 0.1f));
    }

    private IEnumerator End(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}