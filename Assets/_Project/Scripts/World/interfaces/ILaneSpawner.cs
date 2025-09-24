using System.Transactions;
using UnityEngine;

public interface ILaneSpawner
{
    GameObject SpawnLane(int index, float zPos, bool isSafeZone, bool isDense);
    void InitializePool();
    LaneType GetCurrentLaneType();
}
