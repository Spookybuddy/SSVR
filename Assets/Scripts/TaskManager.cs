using UnityEngine;
using TMPro;

[System.Serializable]
public class PuzzleGroups
{
    public PuzzleSystem[] Puzzles;
}

public class TaskManager : MonoBehaviour
{
    private bool completed;
    private bool[] finishedTasks = new bool[3] { false, false, false };
    private int[] completedTasks = new int[3] { 0, 0, 0 };
    [Header("Wires\nVents\nFuel")]
    public PuzzleGroups[] Tasks;
    public TextMeshProUGUI[] checklist;
    public GameObject[] final;
    private string[] checkText = new string[3];

    void Start()
    {
        final[0].SetActive(true);
        final[1].SetActive(false);
        for (int i = 0; i < 3; i++) checkText[i] = checklist[i].text;
    }

    private void FixedUpdate()
    {
        if (!completed) {
            for (int i = 0; i < checklist.Length; i++) {
                //Sum up the number of completed tasks and update the respective text
                completedTasks[i] = 0;
                for (int j = 0; j < Tasks[i].Puzzles.Length; j++) {
                    if (Tasks[i].Puzzles[j].completed) completedTasks[i]++;
                }
                checklist[i].text = checkText[i] + completedTasks[i].ToString() + "/" + (Tasks[i].Puzzles.Length).ToString();
                if (completedTasks[i] == Tasks[i].Puzzles.Length) {
                    checklist[i].fontStyle = FontStyles.Strikethrough;
                    finishedTasks[i] = true;
                } else checklist[i].fontStyle = FontStyles.Normal;
            }
            completed = (finishedTasks[0] && finishedTasks[1] && finishedTasks[2]);

            //You Win!
            if (completed) {
                final[0].SetActive(false);
                final[1].SetActive(true);
            }
        }
    }
}