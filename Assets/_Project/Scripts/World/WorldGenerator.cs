using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    [Header("Dependancies")]
    [SerializeField]
    private ObstacleSpawner obstacleSpawner;

    [Header("Collectables")]
    [SerializeField]
    private GameObject coinPrefab;
    [SerializeField]
    [UnityEngine.Range(0, 1)]
    private float coinSpawnChance = 0.25f; //25% chance to spawn a coin

    [Header("World Generation Settings")]
    [Tooltip("All the possible lane types")]
    [SerializeField]
    private List<LaneType> lanetypes;  //using system.collections.generic

    [Tooltip("The number of lanes to be generated at the start")]
    [SerializeField]
    private int initialLanes = 20;

    [Tooltip("how many lanes to maintian")]
    [SerializeField]
    private int maxLanesinscene = 30;

    //private variables
    private float currentZpos = 0f;
    private Queue<GameObject> activeLanes = new Queue<GameObject>(); //keeping track of active lanes

    private void OnEnable()
    {
        PlayerControler.OnPlayerMovedForward += HandlePlayerMovedForward;
    }

    private void OnDisable()
    {
        PlayerControler.OnPlayerMovedForward -= HandlePlayerMovedForward;
    }

    private void Start()
    {
        InitializePool();
        InitializeWorld();
    }

    private void InitializePool()
    {
        foreach (var lanetype in lanetypes)
        {
            //using the obj pooler to create pools for each lane type
            ObjectPooler.Instance.CreatePool(lanetype.laneName, lanetype.lanePrefab, maxLanesinscene);
        }
    }

    //spawns initial set of lanes at the start of the game
    private void InitializeWorld()
    {
        //spawnning a few lanes in the start
        for (int i = 0; i < 3; i++)
        {
            spawnedLane(0);  //first 3 lanes are always grass lanes
        }

        //spawnning random lanes
        for (int i = 3; i < initialLanes; i++)
        {
            spawnedLane();
        }
    }

    //when the player moves forward this function is called to spawn a new lane
    private void HandlePlayerMovedForward()
    {
        //spawn a new lane
        spawnedLane();

        //removin the oldest lane if we have more than maxLanesinscene
        if (activeLanes.Count > maxLanesinscene)
        {
            GameObject oldLane = activeLanes.Dequeue();
            oldLane.SetActive(false);
        }
    }

    private void spawnedLane(int index = -1)
    {
        if (lanetypes.Count == 0)
        {
            Debug.LogError("No lane types assigned in the inspector.");
            return;
        }

        //picking a random lane type if index is not provided
        int randindex = (index == -1) ? Random.Range(0, lanetypes.Count) : index;
        LaneType selectedlanetype = lanetypes[randindex];

        //using the obj pooler to spawn the lane
        GameObject lane = ObjectPooler.Instance.SpawnFromPool(
            selectedlanetype.laneName,
            new Vector3(0, 0, currentZpos),
            Quaternion.identity
        );

        if(lane != null)
        {
            activeLanes.Enqueue(lane); //adding the lane to the queue
            currentZpos++; //assuming each lane has a length of 1 unit

            obstacleSpawner.TrySpawningObstacles(lane, selectedlanetype);

            if(!selectedlanetype.canHaveObstacles)
            {
                if(Random.value <= coinSpawnChance)
                {
                    float randX = Random.Range(-5f, 5f);
                    Vector3 coinPos = new Vector3(randX, lane.transform.position.y + 1f, lane.transform.position.z);
                    Instantiate(coinPrefab, coinPos, Quaternion.identity);
                }
            }
        }
    }

}
