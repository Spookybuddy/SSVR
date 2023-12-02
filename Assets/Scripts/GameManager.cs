using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject title;
    public GameObject credits;
    public GameObject options;

    public void LoadScene()
    {
        StartCoroutine(Load());
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

    private void SetMenus(bool t, bool c, bool o)
    {
        title.SetActive(t);
        credits.SetActive(c);
        options.SetActive(o);
    }

    private IEnumerator Load()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("Solarius_Interior");
        while (!async.isDone) yield return null;
    }
}