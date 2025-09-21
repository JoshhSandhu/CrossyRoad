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

    [Tooltip("lane spawn height")]
    [SerializeField]
    private float laneSpawnHeight = 0f;

    [Tooltip("coin spawn height")]
    [SerializeField] 
    private float coinHeightOffset = 1f;

    private LaneType previousLaneType; //to keep track of the previous lane type
    private int lastLilyPadx = 0;

    //private variables
    private float currentZpos = 0f;
    private Queue<GameObject> activeLanes = new Queue<GameObject>(); //keeping track of active lanes

    private void OnEnable()
    {
        PlayerController.OnPlayerMovedForward += HandlePlayerMovedForward;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerMovedForward -= HandlePlayerMovedForward;
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

    //this script spawns a lane at the current z position
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

        //ensuring that the lily lane type does not spawn consecutively
        if (previousLaneType != null && previousLaneType.name == "LillyWaterPadData" && selectedlanetype.name == "LillyWaterPadData")
        {
            randindex = (index == -1) ? Random.Range(0, lanetypes.Count) : index;
            selectedlanetype = lanetypes[randindex];
        }

        //using the obj pooler to spawn the lane
        GameObject lane = ObjectPooler.Instance.SpawnFromPool(
            selectedlanetype.laneName,
            new Vector3(0, laneSpawnHeight, currentZpos),
            Quaternion.identity
        );

        if(lane != null)
        {
            activeLanes.Enqueue(lane); //adding the lane to the queue
            currentZpos++; //assuming each lane has a length of 1 unit

            

            if(!selectedlanetype.lanePrefab.CompareTag("Water"))
            {
                if(Random.value <= coinSpawnChance)
                {
                    int randX = Random.Range(-5, 5);
                    Vector3 coinPos = new Vector3(randX, lane.transform.position.y + coinHeightOffset, lane.transform.position.z);
                    Instantiate(coinPrefab, coinPos, Quaternion.identity);
                }
            }

            if (selectedlanetype.decorationsPrefab.Length >0)
            {
                List<int> usedXPositions = new List<int>(); //storing the pos where the decorations have already spawned
                
                foreach (GameObject decorationPrefab in selectedlanetype.decorationsPrefab)
                {
                    if (decorationPrefab.CompareTag("Platform"))
                    {
                        int currentX = (previousLaneType != null && previousLaneType.name == "LillyWaterPadData") ? lastLilyPadx : Random.Range(-4,-2);
                        while ( currentX <= 7f)
                        {
                            Vector3 decorationPos = new Vector3(currentX, lane.transform.position.y, lane.transform.position.z);
                            GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
                            newDecoration.transform.SetParent(lane.transform);
                            //remembering the last x pos of the lily pad
                            lastLilyPadx = currentX; 
                            currentX += Random.Range(2, 4);
                        }
                    }
                    else if (decorationPrefab.GetComponent<SignalController>() != null)
                    {
                        Vector3 signalPos = new Vector3(0, lane.transform.position.y, lane.transform.position.z - 0.5f);
                        Quaternion signalRotation = Quaternion.Euler(0, 90, 0);
                        GameObject newSignal = Instantiate(decorationPrefab, signalPos, signalRotation);
                        newSignal.transform.SetParent(lane.transform);
                    }
                    else
                    {
                        int decorationCount = Random.Range(0, 3); //randomly decide how many decorations to spawn
                        for( int i = 0; i < decorationCount; i++)
                        {
                            int randX = Random.Range(-8, 8);
                            //ensuring that no two decorations spawn too close to each other
                            while (usedXPositions.Exists(x => Mathf.Abs(x - randX) < 2))
                            {
                                randX = Random.Range(-8, 8);
                            }
                            usedXPositions.Add(randX);
                            Vector3 decorationPos = new Vector3(randX, lane.transform.position.y + 0.5f, lane.transform.position.z);
                            GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
                            newDecoration.transform.SetParent(lane.transform);
                        }
                    }
				}
			}
            obstacleSpawner.TrySpawningObstacles(lane, selectedlanetype);
        }
        previousLaneType = selectedlanetype; //remembering the previous lane type
    }
}
