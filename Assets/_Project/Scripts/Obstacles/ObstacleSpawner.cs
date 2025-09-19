using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("the min time btw obstacles spawns on a single lane")]
    [SerializeField]
    private float minSpawnTime = 2f;

    [Tooltip("the max time btw obstacles spawns on a single lane")]
    [SerializeField]
    private float maxSpawnTime = 5f;

    [Tooltip("the range on the X axis from where the obstacles spawn from the edge of the screen")]
    [SerializeField]
    private float spawnRangeX = 12f;

    public void TrySpawningObstacles(GameObject lane, LaneType laneType)
    {
        //checking if the lane can have obstacles
        if (!laneType.canHaveObstacles || laneType.obstaclePrefab.Length == 0)
        {
            return; //exiting the method if it can't
        }

        //if the lane can have obstacles, we start spawning them
        StartCoroutine(SpawnObstaclesCoroutine(lane, laneType));
    }

    private System.Collections.IEnumerator SpawnObstaclesCoroutine(GameObject lane, LaneType laneType)
    {
        //randomly decide if the cars will go from
        //left to right: 1 
        //right to left: -1
        int direction = Random.Range(0, 2) == 0 ? 1 : -1;

        //randomly decide a speed for all cars on this lane.
        float speed = Random.Range(5f, 10f);
        float LogSpeed = Random.Range(2f, 7f);
        float Trainspeed = Random.Range(25f, 30f);

        //keep spawning as long as the lane is active
        while (lane.activeInHierarchy)
        {
            //random wait time before next car
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

            //random obstacle
            GameObject obstaclePrefab = laneType.obstaclePrefab[Random.Range(0, laneType.obstaclePrefab.Length)];

            // Determine the spawn position and rotation.
            Vector3 spawnPosition = new Vector3(
                spawnRangeX * -direction,
                lane.transform.position.y + 0.5f,
                lane.transform.position.z
            );
            Quaternion spawnRotation = Quaternion.Euler(0, 90 * direction, 0);

            GameObject spawnedObstacle = Instantiate(obstaclePrefab, spawnPosition, spawnRotation);
            spawnedObstacle.transform.SetParent(lane.transform); // Parent to the lane for organization.
            

            if (spawnedObstacle.TryGetComponent<Car>(out Car car))
            {
                car.speed = speed;
            }
            else if (spawnedObstacle.TryGetComponent<Log>(out Log log))
            {
                log.speed = LogSpeed;
            }
            else if (spawnedObstacle.TryGetComponent<Train>(out Train train))
            {
                train.speed = Trainspeed;
            }
        }
    }
}
