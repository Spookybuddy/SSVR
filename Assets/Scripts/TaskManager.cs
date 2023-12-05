using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public bool completed;
    public PuzzleSystem[] tasks;
    public TextMeshProUGUI[] checklist;

    private void FixedUpdate()
    {
        if (!completed) {
            completed = true;
            for (int i = 0; i < tasks.Length; i++) {
                if (!tasks[i].completed) {
                    completed = false;
                    checklist[i].fontStyle = FontStyles.Normal;
                } else {
                    checklist[i].fontStyle = FontStyles.Strikethrough;
                }
            }
        }
    }
}