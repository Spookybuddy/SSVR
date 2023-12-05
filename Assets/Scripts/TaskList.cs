using UnityEngine;

public class TaskList : MonoBehaviour
{
    public GameObject mainCam;
    public GameObject checklist;
    public float minDistance;
    public float maxDistance;
    public float rotationPadding;

    private float distance;
    public float rotation;

    private bool handRange;
    private bool handRotate;

    void Update()
    {
        distance = Vector3.Distance(mainCam.transform.position, checklist.transform.position);
        rotation = checklist.transform.eulerAngles.z;

        //Check both rotation and distance to display
        handRotate = (rotation > 90 - rotationPadding && rotation < 90 + rotationPadding);
        handRange = (distance > minDistance && distance < maxDistance);
        
        checklist.SetActive(handRange && handRotate);
    }
}
