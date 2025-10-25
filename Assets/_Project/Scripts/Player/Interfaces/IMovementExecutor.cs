using System.Collections;
using UnityEngine;

/// <summary>
/// Interface for executing player movement
/// </summary>
public interface IMovementExecutor
{
    IEnumerator ExecuteMovement(Vector3 destination);
    void FaceDirection(Vector3 direction);
}
