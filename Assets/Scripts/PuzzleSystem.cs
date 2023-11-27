using UnityEngine;

public class PuzzleSystem : MonoBehaviour
{
    public bool completed;
    public Puzzle[] subTasks;
    private bool locked;

    private void FixedUpdate()
    {
        if (!locked) {
            //Check all tasks for completion
            completed = true;
            foreach (Puzzle task in subTasks) {
                if (!task.completed) {
                    completed = false;
                    break;
                }
            }

            //Shut down all puzzles once completed
            if (completed) {
                foreach (Puzzle task in subTasks) {
                    task.locked = true;
                }
                locked = true;
                Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            }
        }
    }
}