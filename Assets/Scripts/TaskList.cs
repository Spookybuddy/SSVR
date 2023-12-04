using UnityEngine;

public class TaskList : MonoBehaviour
{
    public GameObject mainCam;
    public GameObject checklist;
    public float minDistance;
    public float maxDistance;
    public float rotationPadding;

    private float distance;

    public float rotationX;
    public float rotationZ;

    private bool handRange;
    private bool handRotateX;
    private bool handRotateZ;

    void Update()
    {
        distance = Vector3.Distance(mainCam.transform.position, checklist.transform.position);
        rotationZ = checklist.transform.eulerAngles.z;
        rotationX = checklist.transform.eulerAngles.x;

        //Check both rotation and distance to display
        handRotateX = (rotationX > 45 - rotationPadding && rotationX < 45 + rotationPadding);
        handRotateZ = (rotationZ > 90 - rotationPadding && rotationZ < 90 + rotationPadding);
        if (rotationZ > 300) handRotateZ = true;
        handRange = (distance > minDistance && distance < maxDistance);
        
        checklist.SetActive(handRange && (handRotateZ));
    }
}
