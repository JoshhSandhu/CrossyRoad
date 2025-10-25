using UnityEngine;

/// <summary>
/// Handles player movement validation logic
/// </summary>
public class PlayerMovementValidator : IMovementValidator
{
    private readonly Transform playerTransform;
    private readonly float xBoundary;
    private readonly LayerMask blockingMask;
    private readonly LayerMask groundMask;
    private readonly Vector3 blockCastHalfExtents;

    public PlayerMovementValidator(Transform playerTransform, float xBoundary, LayerMask blockingMask, LayerMask groundMask, Vector3 blockCastHalfExtents)
    {
        this.playerTransform = playerTransform;
        this.xBoundary = xBoundary;
        this.blockingMask = blockingMask;
        this.groundMask = groundMask;
        this.blockCastHalfExtents = blockCastHalfExtents;
    }

    public bool CanMove(Vector3 direction)
    {
        Vector3 snappedPos = new Vector3(
            Mathf.Round(playerTransform.position.x),
            playerTransform.position.y + 0.5f,
            Mathf.Round(playerTransform.position.z)
        );

        Vector3 destination = snappedPos + direction;

        if (Mathf.Abs(destination.x) > xBoundary)
        {
            return false;
        }

        //compute expected landing height, and then using it for accurate blocking vs rocks while on logs
        float candidateLandingY = playerTransform.position.y;
        Vector3 probeStart = destination + Vector3.up * 5f;
        if (Physics.Raycast(probeStart, Vector3.down, out var probeHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            candidateLandingY = probeHit.point.y;
        }

        //pre hop check to avoid false positives from nearby solid obstacles
        Vector3 overlapCenter = new Vector3(destination.x, candidateLandingY + 0.6f, destination.z);
        Collider[] hits = Physics.OverlapBox(overlapCenter, blockCastHalfExtents, Quaternion.identity, blockingMask, QueryTriggerInteraction.Ignore);
        //prehop box check with boxcast to catch walls and boundaries reliably
        if (hits != null && hits.Length > 0)
        {
            return false;
        }

        return true;
    }

    public Vector3 ValidateDestination(Vector3 destination)
    {
        //compute expected landing height, and then using it for accurate blocking vs rocks while on logs
        float candidateLandingY = playerTransform.position.y;
        Vector3 probeStart = destination + Vector3.up * 5f;
        if (Physics.Raycast(probeStart, Vector3.down, out var probeHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            candidateLandingY = probeHit.point.y;
        }

        return new Vector3(destination.x, candidateLandingY, destination.z);
    }
}
