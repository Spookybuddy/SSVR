using UnityEngine;

public class Push : MonoBehaviour
{
    private Vector3 start;
    private Vector3 final;
    public Vector3 direction;
    public Transform[] hands;
    public float padding;

    private int closest;
    private bool press;
    private bool hold;
    public float value;
    private int _dir;

    //Save start and end points
    private void Start()
    {
        start = transform.position;
        final = start + direction;
        for (int i = 0; i < 3; i++) {
            if (direction[i] != 0) _dir = i;
        }
    }

    private void FixedUpdate()
    {
        //Check for the distance from both hands to button's center. If close enough it is being pressed
        press = false;
        for (int i = 0; i < hands.Length; i++) {
            if (Vector3.Distance(hands[i].position, transform.position) < padding) {
                closest = i;
                press = true;
                hold = true;
                transform.position = Vector3.Lerp(start, final, value);
            }
        }

        //Maintain press within a certain range even if the hand leaves
        if (Vector3.Distance(hands[0].position, transform.position) > 1.25f * padding && Vector3.Distance(hands[1].position, transform.position) > 1.25f * padding) hold = false;

        //Lerp using the distance the player has passed beyond the press threshold
        if (press) {
            value = Mathf.Clamp01(value + Mathf.Abs(padding - Mathf.Abs(hands[closest].position[_dir] - transform.position[_dir])));
        } else if (!hold) {
            value = Mathf.Clamp01(value - (Time.deltaTime * 2));
            transform.position = Vector3.Lerp(start, final, value);
        }
    }
}