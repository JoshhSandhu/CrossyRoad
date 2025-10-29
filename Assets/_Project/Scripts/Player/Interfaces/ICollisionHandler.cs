using UnityEngine;

/// <summary>
/// Interface for handling player collisions
/// </summary>
public interface ICollisionHandler
{
    void HandleTriggerEnter(Collider other);
    void HandleCollisionEnter(Collision collision);
    void CheckForOutOfBounds();
}
