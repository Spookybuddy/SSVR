using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [Header("0 = Connect\n1 = Remove\n2 = Refuel")]
    public int puzzleType;
    public string tagType;
    public bool completed;
    public bool locked;

    [Header("Refuel Puzzle Components")]
    public Transform meter;
    public Push button;
    public float target;
    public float range;
    public float maximum;
    private float value;
    private Vector3 scale;

    private void Start()
    {
        locked = false;
        completed = false;
        if (meter != null) scale = meter.localScale;
        maximum = Mathf.Max(maximum, 0.1f);
    }

    //Update the values and line for fill puzzle
    private void FixedUpdate()
    {
        if (puzzleType == 2 && !locked) {
            //Update values
            value = Mathf.Clamp(value + (button.value > 0.2f ? Time.deltaTime * button.value * 2 : -Time.deltaTime), 0.05f, maximum);

            //Check ranges (Can fall out of range)
            completed = (value >= target - range && value <= target + range);

            //Update fill;
            meter.localScale = new Vector3(scale.x, scale.y, value / maximum);
        }
    }

    //Check puzzle type and matching tag when entering
    private void OnTriggerEnter(Collider other)
    {
        if (puzzleType == 0 && !locked) {
            if (other.CompareTag(tagType)) {
                completed = true;
                if (TryGetComponent<Wire>(out Wire snap)) {
                    Vector3 save = other.transform.position;
                    Destroy(other.gameObject);
                    snap.Freeze(save);
                }
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