using UnityEngine;

public class PuzzleSystem : MonoBehaviour
{
    public bool completed;
    public Puzzle[] subTasks;

    private void FixedUpdate()
    {
        completed = true;
        foreach (Puzzle task in subTasks) {
            if (!task.completed) completed = false;
        }
    }
}