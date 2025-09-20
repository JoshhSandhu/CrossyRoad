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

        SignalController signal = lane.GetComponentInChildren<SignalController>();
        //keep spawning as long as the lane is active
        while (lane.activeInHierarchy)
        {
            //random wait time before next car
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

            GameObject obstaclePrefab = laneType.obstaclePrefab[Random.Range(0, laneType.obstaclePrefab.Length)];

            if (laneType.warningSignalPrefab != null)
            {
                float warningDuration = 2.5f;

                Vector3 signalPosition = new Vector3(0, lane.transform.position.y, lane.transform.position.z - 0.5f);
                Quaternion signalRotation = Quaternion.Euler(0, 90, 0);
                GameObject signalInstance = Instantiate(laneType.warningSignalPrefab, signalPosition, signalRotation);
                signalInstance.transform.SetParent(lane.transform);

                Light signalLight = signalInstance.GetComponentInChildren<Light>();

                if (signalLight != null)
                {
                    // Start with the light off
                    signalLight.enabled = false;

                    for (int i = 0; i < 5; i++)
                    {
                        signalLight.enabled = true;
                        yield return new WaitForSeconds(warningDuration / 10);
                        signalLight.enabled = false;
                        yield return new WaitForSeconds(warningDuration / 10);
                    }
                }
                else
                {
                    Debug.LogWarning("Warning Signal Prefab does not have a Light component in its children!");
                    yield return new WaitForSeconds(warningDuration);
                }
                Destroy(signalInstance);

                Vector3 spawnPosition = new Vector3(spawnRangeX * -direction, lane.transform.position.y + 0.5f, lane.transform.position.z);
                Quaternion spawnRotation = Quaternion.Euler(0, 90 * direction, 0);

                GameObject spawnedObstacle = Instantiate(obstaclePrefab, spawnPosition, spawnRotation);
                spawnedObstacle.transform.SetParent(lane.transform);

                if (spawnedObstacle.TryGetComponent<Train>(out Train train))
                {
                    train.speed = Trainspeed;
                }
            }
            else
            {
                

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
                    log.speed = LogSpeed;
                }
            }
        }
    }
}
