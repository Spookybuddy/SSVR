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
    public TaskList[] final; 
    private string[] checkText = new string[] { "- Fix Wires -", "- Clean Vents -", "- Adjust Fuel -\n- Levels -" };

    void Start()
    {
        for (int i = 0; i < 2; i++) final[i].shown = (i == 0);
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
                checklist[i].text = checkText[i] + "\n" + completedTasks[i].ToString() + "/" + (Tasks[i].Puzzles.Length).ToString();
                if (completedTasks[i] == Tasks[i].Puzzles.Length) {
                    checklist[i].fontStyle = FontStyles.Strikethrough;
                    finishedTasks[i] = true;
                } else checklist[i].fontStyle = FontStyles.Normal;
            }
            completed = (finishedTasks[0] && finishedTasks[1] && finishedTasks[2]);

            //You Win
            if (completed) {
                final[0].shown = false;
                final[1].shown = true;
            }
        }
    }
}