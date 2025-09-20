using UnityEngine;

public class BoundaryFollow : MonoBehaviour
{
    [Tooltip("The camera to follow on the Z-axis")]
    public Transform cameraTransform;

    private float initialX;
    private float initialY;

    void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform is not assigned");
            return;
        }
        // Store the starting X and Y position of the container
        initialX = transform.position.x;
        initialY = transform.position.y;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Create the new position
        Vector3 newPosition = new Vector3(
            initialX,
            initialY, 
            cameraTransform.position.z 
        );

        transform.position = newPosition;
    }
}