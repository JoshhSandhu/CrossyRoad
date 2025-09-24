using UnityEngine;

public interface IDecorationSpawner
{
    void SpawnDecorations(GameObject lane, LaneType laneType, bool isSafeZone, bool isDense);
}