using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [Tooltip("the player that the camera will follow")]
    public Transform player;

    [Tooltip("smoothness of the camera follow")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    //distance btw the camera and the player
    public Vector3 offset;

    //start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - player.position; //initial offset
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset; //to follow on both x and z axis

        //smoothly following the player
        Vector3 smoothPosition = Vector3.Lerp( transform.position, desiredPosition, smoothSpeed);
        transform.position = new Vector3(smoothPosition.x, transform.position.y, smoothPosition.z);
    }
}
