using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [Tooltip("the player that the camera will follow")]
    public Transform player;

    [Header("Damping")]
    [Tooltip("how fast the cam moves to catch up on the x movement")]
    [Range(0.1f, 20f)] public float Xdamping = 8f;
    [Tooltip("how fast the cam advances on the Z axis, the cam does not move backward")]
    [Range(0.1f, 20f)] public float Zdamping = 5f;

    [Header("Offsets")]
    [Tooltip("Cam offset relative to the player start")]
    public Vector3 offset;
    [Tooltip("extra look ahead on the Z axis")]
    public float Zlookahead = 1.5f;
    [Tooltip("clamping the x movement on the X axis")]
    public float Xfollowclamp = 1f;
    [Tooltip("world center to keep centered")]
    [Range(0f, 1f)] public float Xworldcentere = 0f;
    [Tooltip("0 = center only, 1=follow player")]
    [Range(0f, 1f)] public float XfollowWeight = 0.6f;

    [Header("Screen Shake")]
    [Tooltip("how fast shake fades per second")]
    public float traumaDecay = 1.5f;
    [Tooltip("max positional shake offset in world units")]
    public float maxShakeOffset = 0.2f;
    [Tooltip("max rotational shake in degrees")]
    public float maxShakeAngle = 2f;
    private float trauma = 0f;
    public bool CameraShake = true;

    [Header("Tilt")]
    [Tooltip("downward tilt in degrees")]
    public float pitchDegrees = 55f;
    [Tooltip("Slight world yaw in degrees to give diagonal feel")]
    public float yawDegrees = -20f;

    private float initialY;
    private float maxZ;
    private Quaternion baseRotation;

    //start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - player.position; //initial offset
        initialY = transform.position.y;
        maxZ = player != null ? player.position.z : 0f;
        baseRotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
        transform.rotation = baseRotation;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        //adv only to the furthest z pos reached
        maxZ = Mathf.Max(maxZ, player.position.z);

        //the target Z includes a lil look back and the look ahead
        float targetZ = maxZ + offset.z + Zlookahead;
        float newZ = Mathf.Lerp(transform.position.z, targetZ, Time.deltaTime * Zdamping);

        //to never move the cam back on the Z pos
        if (newZ < transform.position.z)
        {
            newZ = transform.position.z;
        }

        //the lateral follow of the player on the X axis
        //blend btw world center and player X for better centering
        float targetX = player.position.x + offset.x;
        float centertargetX = Xworldcentere + offset.x;
        float playertargetX = player.position.x + offset.x;
        float targX = Mathf.Lerp(centertargetX, playertargetX, XfollowWeight);
        if (Xfollowclamp > 0f)
        {
            float baseX = offset.x;
            float playerRelativeX = player.position.x + baseX;
            targetX = Mathf.Clamp(targetX, centertargetX - Xfollowclamp, centertargetX + Xfollowclamp);
        }

        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * Xdamping);
        Vector3 finalPos = new Vector3(newX, initialY, newZ);

        if (CameraShake)
        {
            if (trauma > 0f)
            {
                float t = trauma * trauma;
                Vector3 offsetShake = new Vector3(
                    (Random.value * 2f - 1f) * maxShakeOffset * t,
                    (Random.value * 2f - 1f) * maxShakeOffset * 0.5f * t,
                    (Random.value * 2f - 1f) * maxShakeOffset * t
                );
                finalPos += offsetShake;

                Vector3 ang = new Vector3(
                    (Random.value * 2f - 1f) * maxShakeAngle * t,
                    (Random.value * 2f - 1f) * maxShakeAngle * t,
                    (Random.value * 2f - 1f) * maxShakeAngle * t
                );
                transform.rotation = baseRotation * Quaternion.Euler(ang);

                trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);
            }
            else
            {
                transform.rotation = baseRotation;
            }
        }

        transform.position = finalPos;
    }

    //this function triggers the shake
    public void Shake(float amount = 0.5f)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    public void ResetCamera()
    {
        Debug.Log("Resetting camera state...");

        maxZ = player != null ? player.position.z : 0f;

        if (player != null)
        {
            Vector3 resetPosition = player.position + offset;
            resetPosition.y = initialY;
            transform.position = resetPosition;
        }

        transform.rotation = baseRotation;

        trauma = 0f;

        Debug.Log("Camera reset complete!");
    }
}
