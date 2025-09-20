using TMPro;
using UnityEngine;

public class Log : MonoBehaviour
{
    [Header("speeds")]
    public float SlowSpeed = 3f;
    public float fastSpeed = 9f;

    [Header("Broundries")]
    [Tooltip("the x value defines the edge of the playable area")]
    public float boundaryX = 9f;
    [Tooltip("how far the log in the front is")]
    [SerializeField]
    private float raycastDistance = 1.5f;
    private float colliderHalfLength;
    private CapsuleCollider logCollider;

    public float currentSpeed{get; private set; }

    void Start()
    {
        //get the size of the capsule collider to calculate the offset
        logCollider = GetComponent<CapsuleCollider>();
        if (logCollider != null)
        {
            //the capsule's height is its length since we set its direction to Z-Axis
            colliderHalfLength = logCollider.height / 2;
        }
    }

    void FixedUpdate()
    {
        //our speed based on out default position
        float speedByPos = (Mathf.Abs(transform.position.x) > boundaryX) ? fastSpeed : SlowSpeed;
        currentSpeed = speedByPos;

        //getting the center of the collider in world space
        Vector3 colliderCenter = transform.TransformPoint(logCollider.center);
        //calculating the raycast origin at the front of the log
        Vector3 raycastOrigin = colliderCenter + (transform.forward * colliderHalfLength);

        //drawing a raycat to debug the issue
        //Debug.DrawRay(raycastOrigin, transform.forward * raycastDistance, Color.red);
        //cast a ray to check who is in front of the log
        RaycastHit hit;
        if(Physics.Raycast(raycastOrigin, transform.forward, out hit, raycastDistance))
        {
            //if there is another log in front of this log, match its speed
            if (hit.collider.TryGetComponent<Log>(out Log frontLog))
            {
                //change the speed to match the log in front
                currentSpeed = frontLog.currentSpeed;
            }
        }
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}