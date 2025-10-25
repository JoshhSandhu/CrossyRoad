using UnityEngine;

/// <summary>
/// Interface for validating player movement
/// </summary>
public interface IMovementValidator
{
    bool CanMove(Vector3 direction);
    Vector3 ValidateDestination(Vector3 destination);
}
