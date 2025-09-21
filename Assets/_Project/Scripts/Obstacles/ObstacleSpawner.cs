using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("the range on the X axis from where the obstacles spawn from the edge of the screen")]
    [SerializeField]
    private float spawnRangeX = 20f;

    [Header("Collectibles")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField][Range(0, 1)] private float coinSpawnChance = 0.5f; //50% chance for a coin to be on a log

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

        SignalController signal = lane.GetComponentInChildren<SignalController>();
        //keep spawning as long as the lane is active
        while (lane.activeInHierarchy)
        {
            //random wait time before next car
            yield return new WaitForSeconds(Random.Range(laneType.minSpawnTime, laneType.maxSpawnTime));

            GameObject obstaclePrefab = laneType.obstaclePrefab[Random.Range(0, laneType.obstaclePrefab.Length)];

            if (signal != null)
            {
                float warningDuration = 2.5f; //duration of the warning signal

                signal.StartCoroutine(signal.FlashWarning(warningDuration));

                yield return new WaitForSeconds(warningDuration);

            }
              
            Vector3 spawnPosition = new Vector3(spawnRangeX * -direction, lane.transform.position.y + 0.5f, lane.transform.position.z);
            Quaternion spawnRotation = Quaternion.Euler(0, 90 * direction, 0);

            GameObject spawnedObstacle = Instantiate(obstaclePrefab, spawnPosition, spawnRotation);
            spawnedObstacle.transform.SetParent(lane.transform);


            if (spawnedObstacle.TryGetComponent<Car>(out Car car))
            {
                car.speed = speed;
            }
            else if (spawnedObstacle.TryGetComponent<Log>(out Log log))
            {
                float baseSpeed = Random.Range(2f, 4f);
                log.SlowSpeed = baseSpeed;
                log.fastSpeed = baseSpeed * 3f;

                if(Random.value <= coinSpawnChance)
                {
                    Vector3 coinPos = spawnedObstacle.transform.position + new Vector3(0, -0.2f, 0);
                    Instantiate(coinPrefab, coinPos, Quaternion.identity, spawnedObstacle.transform);
                }
            }
            else if (spawnedObstacle.TryGetComponent<Train>(out Train train))
            {
                train.speed = Trainspeed;
            }
        }
    }
}
