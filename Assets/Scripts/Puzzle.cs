using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [Header("0 = Connect\n1 = Remove\n2 = Refuel")]
    public int puzzleType;
    public string tagType;
    public bool completed;
    public bool locked;

    [Header("Refuel Puzzle Components")]
    public LineRenderer meter;
    public Push button;
    public float target;
    public float range;
    private float value;
    private Vector3 end;

    private void Start()
    {
        locked = false;
        completed = false;
        if (meter != null) {
            end = meter.GetPosition(0);
            meter.SetPosition(1, end);
        }
    }

    //Update the values and line for fill puzzle
    private void FixedUpdate()
    {
        if (puzzleType == 2 && !locked) {
            //Update values
            value = Mathf.Clamp(value + (button.threshold ? Time.deltaTime : -Time.deltaTime), 0, 5);

            //Check ranges (Can fall out of range)
            completed = (value >= target - range && value <= target + range);

            //Update line;
            meter.SetPosition(1, new Vector3(end.x, value / 5 + end.y, end.z));
        }
    }

    //Check puzzle type and matching tag when entering
    private void OnTriggerEnter(Collider other)
    {
        if (puzzleType == 0 && !locked) {
            if (other.CompareTag(tagType)) {
                completed = true;
            }
        }
    }

    //Check puzzle type and matching tag when exiting
    private void OnTriggerExit(Collider other)
    {
        if (puzzleType == 1 && !locked) {
            if (other.CompareTag(tagType)) {
                completed = true;
            }
        }
    }
}