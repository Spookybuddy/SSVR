using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public bool completed;
    public PuzzleSystem[] tasks;

    private void FixedUpdate()
    {
        if (!completed) {
            completed = true;
            foreach (PuzzleSystem task in tasks) {
                if (!task.completed) {
                    completed = false;
                    break;
                }
            }
        }
    }
}