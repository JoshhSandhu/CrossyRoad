using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// main world generator class that orchestrates the creation and management of game lanes.
/// follows SOLID principles by delegating specific responsibilities to specialized components.
/// </summary>
public class WorldGenerator : MonoBehaviour, IWorldGenerator
{
    [Header("Dependencies")]
    [SerializeField] private LaneSpawner laneSpawner;                   // Handles lane spawning logic
    [SerializeField] private CollectableSpawner collectableSpawner;     // Manages collectable spawning
    [SerializeField] private DecorationSpawner decorationSpawner;       // Handles decoration spawning
    [SerializeField] private LaneManager laneManager;                   // Manages active lanes queue
    [SerializeField] private ObstacleSpawner obstacleSpawner;           // Spawns obstacles on lanes

    [Header("World Generation Settings")]
    [Tooltip("The number of lanes to be generated at the start")]
    [SerializeField] private int initialLanes = 20;

    //current Z position for lane spawning. Increments as new lanes are created.
    private float currentZPos = 0f;

    //subscribe to player movement events when the component is enabled.
    private void OnEnable()
    {
        PlayerController.OnPlayerMovedForward += HandlePlayerMovedForward;
    }

    //unsubscribe from player movement events when the component is disabled.
    private void OnDisable()
    {
        PlayerController.OnPlayerMovedForward -= HandlePlayerMovedForward;
    }

    //initialize the world when the game starts
    private void Start()
    {
        InitializePools();
        InitializeWorld();
    }

    private void InitializePools()
    {
        if (laneSpawner != null)
        {
            laneSpawner.Initializepool();
        }
        else
        {
            Debug.LogError("Cannot initialize pools - LaneSpawner is not assigned!");
        }
    }

    //spawns the initial set of lanes at the start of the game
    public void InitializeWorld()
    {
        //2 layers of densely populated lanes
        for (int z = -6; z < -4; z++)
        {
            SpawnLane(0, z, false, true);
        }
        //4 grass lanes behind the player
        for (int z = -4; z < 0; z++)
        {
            SpawnLane(0, z, true, false);
        }

        //spawning a few lanes in the start
        for (int i = 0; i < 3; i++)
        {
            SpawnLane(0, i, true, false);  //first 3 lanes are always grass lanes
        }

        currentZPos = 3;
        //spawning random lanes
        for (int i = 3; i < initialLanes; i++)
        {
            SpawnLane();
        }
    }

    //handles player movement forward by spawning new lanes and managing lane cleanup.
    public void HandlePlayerMovedForward()
    {
        //spawn a new lane
        SpawnLane();

        //remove the oldest lane if we have more than max lanes
        if (laneManager.ShouldRemoveLane())
        {
            laneManager.RemoveOldestLane();
        }
    }

    //spawns a lane at the specified position with given parameters.
    private void SpawnLane(int index = -1, float? zPos = null, bool isSafeZone = false, bool isDense = false)
    {
        float spawnZ = zPos ?? currentZPos;

        GameObject lane = laneSpawner.SpawnLane(index, spawnZ, isSafeZone, isDense);

        if (lane != null)
        {
            laneManager.AddLane(lane);
            currentZPos++;

            //only increment the global z pos if we are not using an override
            if (zPos != null)
            {
                currentZPos++;
            }

            //get the lane type for spawning collectables and decorations
            LaneType laneType = laneSpawner.GetCurrentLaneType();

            //spawn collectables
            collectableSpawner.TrySpawnCollectable(lane, laneType);

            //spawn decorations
            decorationSpawner.SpawnDecorations(lane, laneType, isSafeZone, isDense);

            //spawn obstacles
            obstacleSpawner.TrySpawningObstacles(lane, laneType);
        }
    }
}
