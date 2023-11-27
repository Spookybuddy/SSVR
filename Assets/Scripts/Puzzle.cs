using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [Header("0 = Connect\n1 = Remove\n2 = Levels")]
    public int puzzleType;
    public string tagType;
    public bool completed;
    private bool filling;
    private float value;

    private void Start()
    {
        completed = false;
    }

    private void FixedUpdate()
    {
        if (puzzleType == 2) {

        }
    }

    public void Press()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (puzzleType == 0) {
            if (other.CompareTag(tagType)) {
                completed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (puzzleType == 1) {
            if (other.CompareTag(tagType)) {
                completed = true;
            }
        }
    }
}