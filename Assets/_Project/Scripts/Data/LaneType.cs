using UnityEngine;

[CreateAssetMenu(fileName = "NewLaneType", menuName = "Crossy/Lane")] //this way we can create instances of this asset
public class LaneType : ScriptableObject
{
    [Tooltip("Give the lane a name")]
    public string laneName;

    [Tooltip("The prefab that will be spawned for this lane")]
    public GameObject lanePrefab;

    [Header("obstacle settings")]
    [Tooltip("can obstacles spawn on this lane?")]
    public bool canHaveObstacles = false;

    [Tooltip("the obstecle prefab that will spawn on this lane")]
    public GameObject[] obstaclePrefab;

    [Header("Decorations settings")]
    [Tooltip("decorations to spawn on this lane")]
    public GameObject[] decorationsPrefab;

    [Header("Spawn Settings")]
    [Tooltip("the min time btw each spawn")]
    public float minSpawnTime = 2f;

    [Tooltip("the max time btw each spawn")]
    public float maxSpawnTime = 5f;

    //this is not being used right now
    [Header("warning signal settings")]
    [Tooltip("the warnging signal for the train lane")]
    public GameObject warningSignalPrefab;
}
