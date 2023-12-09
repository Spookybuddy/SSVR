using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject title;
    public GameObject credits;
    public GameObject options;

    public float fadeDuration;
    public Material color;
    private bool inOut;
    private float alpha;
    private Vector4 shade = new (0, 0, 0, 1);

    private void Start()
    {
        alpha = fadeDuration;
        inOut = true;
        SetMenus(true, false, false);
        StartCoroutine(FadeIn());
    }

    public void LoadScene()
    {
        inOut = false;
        StartCoroutine(FadeOut());
    }

    public void OpenMenu(int menu)
    {
        switch (menu) {
            case 1:
                SetMenus(false, true, false);
                break;
            case 2:
                SetMenus(false, false, true);
                break;
            default:
                SetMenus(true, false, false);
                break;
        }
    }

    //Set active menus
    private void SetMenus(bool t, bool c, bool o)
    {
        title.SetActive(t);
        credits.SetActive(c);
        options.SetActive(o);
    }

    //Black -> clear
    private IEnumerator FadeIn()
    {
        while (alpha > 0 && inOut) {
            alpha -= Time.deltaTime;
            shade.w = Mathf.Clamp01(alpha / fadeDuration);
            color.color = shade;
            yield return null;
        }
    }

    //Clear -> black with async scene load
    private IEnumerator FadeOut()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
        async.allowSceneActivation = false;
        while (alpha < fadeDuration) {
            alpha += Time.deltaTime;
            shade.w = Mathf.Clamp01(alpha / fadeDuration);
            color.color = shade;
            yield return null;
        }
        if (async.progress >= 0.9f) async.allowSceneActivation = true;
        else yield return null;
    }
}