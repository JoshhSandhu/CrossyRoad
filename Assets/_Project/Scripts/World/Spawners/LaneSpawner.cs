using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// handles the spawning of lanes
/// uses obj pooling
/// manages lane selection and ensures proper lane sequencing
/// </summary>

public class LaneSpawner : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField] private ObjectPooler objectPooler;

    [Header("world gen")]
    [SerializeField] private List<LaneType> laneTypes;      //lanes for spawning
    [SerializeField] private int maxLanesInScene = 30;      //max number of lanes to maintain
    [SerializeField] private float laneSpawnHeight = 0f;    //the height of the spawned lanes

    private LaneType previousLaneType;
    private LaneType currentLaneType;

    //initializes the obj pool for all the lane types
    public void Initializepool()
    {

        //debugs
        Debug.Log("LaneSpawner.InitializePool() called");
        if (objectPooler == null)
        {
            Debug.LogError("objectPooler is not assigned in LaneSpawner!");
            return;
        }
        if (laneTypes == null || laneTypes.Count == 0)
        {
            Debug.LogError("no lane types assigned in LaneSpawner!");
            return;
        }
        Debug.Log($"Creating pools for {laneTypes.Count} lane types:");
        //end

        foreach (var lanes in laneTypes)
        {
            objectPooler.CreatePool(lanes.laneName, lanes.lanePrefab, maxLanesInScene);
        }
        Debug.Log("All pools created successfully!");
    }

    public GameObject SpawnLane(int index = -1, float? zPos = null, bool isSafeZone = false, bool isDense = false)
    {
        if (objectPooler == null)
        {
            Debug.LogError("ObjectPooler is not assigned in LaneSpawner!");
            return null;
        }
        if (laneTypes.Count == 0)
        {
            Debug.LogError("no lane types assigned in the inspector");
        }

        int randIndex = (index == -1) ? Random.Range(0, laneTypes.Count) : index;
        LaneType selectedLaneType = laneTypes[randIndex];

        if (previousLaneType != null && previousLaneType.name == "LillyWatePadData" && selectedLaneType.name == "LillyWatePadData")
        {
            randIndex = (index == -1) ? Random.Range(0, laneTypes.Count) : index;
            selectedLaneType = laneTypes[randIndex];
        }

        float spawnZ = zPos ?? 0f;
        GameObject lane = objectPooler.SpawnFromPool(
            selectedLaneType.laneName,
            new Vector3(0, laneSpawnHeight, spawnZ),
            Quaternion.identity
        );

        if (lane != null) 
        {
            currentLaneType = selectedLaneType;
            previousLaneType = selectedLaneType;
        } 

        return lane;
    }

    public LaneType GetCurrentLaneType()
    {
        return currentLaneType;
    }
}
