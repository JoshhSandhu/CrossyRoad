using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player movement execution and animation
/// </summary>
public class PlayerMovementExecutor : IMovementExecutor
{
    private readonly Transform playerTransform;
    private readonly Rigidbody playerRb;
    private readonly float hopHeight;
    private readonly float hopDuration;
    private readonly LayerMask groundMask;
    private Quaternion targetRotation;

    public PlayerMovementExecutor(Transform playerTransform, Rigidbody playerRb, float hopHeight, float hopDuration, LayerMask groundMask)
    {
        this.playerTransform = playerTransform;
        this.playerRb = playerRb;
        this.hopHeight = hopHeight;
        this.hopDuration = hopDuration;
        this.groundMask = groundMask;
    }

    public IEnumerator ExecuteMovement(Vector3 destinationXZ)
    {
        //Debug.Log($"HopCoroutine started for destination: {destinationXZ}");
        playerTransform.SetParent(null);
        float landingY = destinationXZ.y;

        Vector3 rayStart = destinationXZ + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out var groundHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            landingY = groundHit.point.y;
        }

        Vector3 startPos = playerRb.position;
        Vector3 destination = new Vector3(destinationXZ.x, landingY, destinationXZ.z);
        float elapsedTime = 0f;
        //Debug.Log($"HopCoroutine: startPos={startPos}, destination={destination}, parent={playerTransform.parent?.name ?? "null"}, rotation={playerTransform.rotation.eulerAngles}");
        while (elapsedTime < hopDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float a = Mathf.Clamp01(elapsedTime / hopDuration);
            float arc = Mathf.Sin(a * Mathf.PI) * hopHeight;
            Vector3 pos = Vector3.Lerp(startPos, destination, a);
            pos.y += arc;
            playerRb.MovePosition(pos);
            //if (smoothTurn) { playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime); }
            //playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        playerRb.MovePosition(destination);
        playerTransform.rotation = targetRotation;
        //if we landed on a log
        if (Physics.Raycast(destination + Vector3.up * 0.5f, Vector3.down, out var landinghit, 1.5f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (landinghit.collider.CompareTag("Platform"))
            {
                playerTransform.SetParent(landinghit.transform);
            }
            else if (landinghit.collider.CompareTag("Water"))
            {
                // This will be handled by the collision handler
            }
            else
            {
                playerTransform.SetParent(null);
            }
        }
        //Debug.Log($"HopCoroutine completed, isMoving set to false");
    }

    public void FaceDirection(Vector3 dir)
    {
        Vector3 flat = new Vector3(dir.x, 0f, dir.z);
        if (dir == Vector3.zero)
        {
            return;
        }
        Quaternion rawTarget = Quaternion.LookRotation(flat, Vector3.up);
        float yaw = rawTarget.eulerAngles.y;
        float snappedYaw = Mathf.Round(yaw / 90f) * 90f;
        targetRotation = Quaternion.Euler(0f, snappedYaw, 0f);
        //Debug.Log($"FaceDirection: dir={dir}, yaw={yaw}, snappedYaw={snappedYaw}, targetRotation={targetRotation.eulerAngles}");
        //if (smoothTurn)
        //{
        //    playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    playerTransform.rotation = targetRotation;
        //}
        playerTransform.rotation = targetRotation;
        //Debug.Log($"FaceDirection: Set rotation to {playerTransform.rotation.eulerAngles}");
    }
}
