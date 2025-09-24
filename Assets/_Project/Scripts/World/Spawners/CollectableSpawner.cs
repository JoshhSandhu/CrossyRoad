using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles the spawn of collectables like the coins on the lanes
/// manages the chance of spawn and location of spawn
/// </summary>
public class CollectableSpawner : MonoBehaviour, ICollectableSpawner
{
    [Header("colectables")]
    [SerializeField] private GameObject coinPrefab;                         //coin prefab
    [SerializeField][Range(0, 1)] private float coinSpawnChance = 0.25f;    //25% chance of spawning a coin
    [SerializeField] private float coinHeightOffset = 1f;                   //height offset of the coin

    //this attempts to spawn the game objects based on the chance and the lane type
    public void TrySpawnCollectable(GameObject lane, LaneType laneType)
    {
        if (!laneType.lanePrefab.CompareTag("Water"))
        {
            if (Random.value < coinSpawnChance)
            {
                int randX = Random.Range(-5, 5);
                Vector3 coinPos = new Vector3(randX, lane.transform.position.y + coinHeightOffset, lane.transform.position.z);
                Instantiate(coinPrefab, coinPos, Quaternion.identity);
            }
        }
    }
}
