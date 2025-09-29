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

    [Header("Spawn Area")]
    [Tooltip("The spawn area prefab to instantiate at the start")]
    [SerializeField] private GameObject spawnAreaPrefab;
    [Tooltip("Distance the player needs to move forward before spawn area despawns")]
    [SerializeField] private float spawnAreaDespawnDistance = 5f;

    //current Z position for lane spawning. Increments as new lanes are created.
    private float currentZPos = 0f;
    private GameObject spawnAreaInstance;

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
        //spawn the spawn area prefab at the specified position
        if (spawnAreaPrefab != null)
        {
            spawnAreaInstance = Instantiate(spawnAreaPrefab, new Vector3(0, 0.1f, 0), Quaternion.identity);
            Debug.Log("Spawn area prefab instantiated at (0, 0.1, 0)");
        }
        else
        {
            Debug.LogWarning("Spawn area prefab is not assigned in WorldGenerator!");
        }

        //set current Z position to 2 for normal lane spawning
        currentZPos = 3;

        //spawning random lanes starting from Z position 2
        for (int i = 2; i < initialLanes; i++)
        {
            SpawnLane();
        }
    }

    //handles player movement forward by spawning new lanes and managing lane cleanup.
    public void HandlePlayerMovedForward()
    {
        //check if spawn area should be despawned
        if (spawnAreaInstance != null && GameManager.Instance.playerTransform != null)
        {
            float playerZ = GameManager.Instance.playerTransform.position.z;
            if (playerZ >= spawnAreaDespawnDistance)
            {
                Destroy(spawnAreaInstance);
                spawnAreaInstance = null;
                Debug.Log("Spawn area despawned - player moved significantly forward");
            }
        }
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
