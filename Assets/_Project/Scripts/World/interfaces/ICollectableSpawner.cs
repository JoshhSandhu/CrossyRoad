using UnityEngine;

public interface ICollectableSpawner
{
    void TrySpawnCollectable(GameObject lane, LaneType laneType);
}
