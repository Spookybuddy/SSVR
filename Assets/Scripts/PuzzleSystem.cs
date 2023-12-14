using UnityEngine;

public class PuzzleSystem : MonoBehaviour
{
    public bool completed;
    public Puzzle[] subTasks;
    public AudioSource jingle;
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
                jingle.Play();
            }
        }
    }

    //Restore a broken puzzle when destroying objects
    public void Restore(Puzzle fix)
    {
        for (int i = 0; i < subTasks.Length; i++) {
            if (subTasks[i] == null) {
                subTasks[i] = fix;
                return;
            }
        }
    }
}