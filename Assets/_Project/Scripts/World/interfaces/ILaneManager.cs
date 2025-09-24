using UnityEngine;

public interface ILaneManager
{
    void AddLane(GameObject lane);
    void RemoveOldestLane();
    bool ShouldRemoveLane();
}