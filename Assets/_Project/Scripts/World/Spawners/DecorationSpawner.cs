using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// this file handels the spawning of decorations on the lanes
/// manages different decorations types
/// </summary>

public class DecorationSpawner : MonoBehaviour, IDecorationSpawner
{
    private LaneType previousLaneType;      //tracks previous lane type
    private int lastLilyPadX = 0;           //stores the last pos of the lily pad


    //spawns the decoration based on the lane type and spawn parmeters
    //handels different decorations with correct spawn logic
    public void SpawnDecorations(GameObject lane, LaneType laneType, bool isSafeZone, bool isDense)
    {
        if (laneType.decorationsPrefab.Length == 0)
        {
            return;
        }

        List<int> usedXpos = new List<int>();

        foreach (GameObject decorationPrefab in laneType.decorationsPrefab)
        {
            if (decorationPrefab.CompareTag("Platform"))
            {
                SpawnPlatformDecorations(lane, decorationPrefab);
            }
            else if (decorationPrefab.GetComponent<SignalController>() != null)
            {
                SpawnSignalDecoration(lane, decorationPrefab);
            }
            else
            {
                SpawnRegularDecorations(lane, decorationPrefab, isSafeZone, isDense, usedXpos);
            }
        }
        previousLaneType = laneType;
    }

    //spawns platform decorations with proper spacing
    private void SpawnPlatformDecorations(GameObject lane, GameObject decorationPrefab)
    {
        int currentX = (previousLaneType != null && previousLaneType.name == "LillyWaterPadData" ? lastLilyPadX : Random.Range(-4, 2));

        while (currentX <= 3)
        {
            if (currentX >= -3)
            {
                Vector3 decorationPos = new Vector3(currentX, lane.transform.position.y, lane.transform.position.z);
                GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
                newDecoration.transform.SetParent(lane.transform);
                lastLilyPadX = currentX;
            }
            currentX += Random.Range(2, 4);
        }
    }

    //spawns signal decoration at a specific fixed postions
    private void SpawnSignalDecoration(GameObject lane, GameObject decorationPrefab)
    {
        Vector3 signalPos = new Vector3(0, lane.transform.position.y, lane.transform.position.z - 0.5f);
        Quaternion signalRotation = Quaternion.Euler(0, 90, 0);
        GameObject newSignal = Instantiate(decorationPrefab, signalPos, signalRotation);
        newSignal.transform.SetParent(lane.transform);
    }


    //spawns regual decorations based on lane types
    private void SpawnRegularDecorations(GameObject lane, GameObject decorationPrefab, bool isSafeZone, bool isDense, List<int> usedXPositions) 
    {
        if (isDense)
        {
            SpawnDenseDecorations(lane, decorationPrefab);
        }
        else if (isSafeZone)
        {
            SpawnSafeZoneDecorations(lane, decorationPrefab);
        }
        else
        {
            SpawnNormalDecorations(lane, decorationPrefab, usedXPositions);
        }
    }

    //this function spawns decorations densly up to 75%
    private void SpawnDenseDecorations(GameObject lane, GameObject decorationPrefab)
    {
        for (int x = -5; x <= 5; x++)
        {
            if (Random.value < 0.75f)
            {
                GameObject treeObj = Instantiate(decorationPrefab, new Vector3(x, lane.transform.position.y + 0.5f, lane.transform.position.z), Quaternion.identity);
                treeObj.transform.SetParent(lane.transform);
            }
        }
    }

    //spawns decorations sparsly 20%
    private void SpawnSafeZoneDecorations(GameObject lane, GameObject decorationPrefab)
    {
        if (Random.value < 0.2f)
        {
            int randX = Random.Range(-5, 5);
            Vector3 decorationPos = new Vector3(randX, lane.transform.position.y + 0.5f, lane.transform.position.z);
            GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
            newDecoration.transform.SetParent(lane.transform);
        }
    }

    //spawns decorations randomly with random pos and collision avoidance
    private void SpawnNormalDecorations(GameObject lane, GameObject decorationPrefab, List<int> usedXPositions)
    {
        int decorationCount = Random.Range(0, 3);

        for (int i = 0; i < decorationCount; i++)
        {
            int randX = Random.Range(-5, 5);

            while (usedXPositions.Exists(x => Mathf.Abs(x - randX) < 2))
            {
                randX = Random.Range(-5, 5);
            }

            usedXPositions.Add(randX);
            Vector3 decorationPos = new Vector3(randX, lane.transform.position.y + 0.5f, lane.transform.position.z);
            GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
            newDecoration.transform.SetParent(lane.transform);
        }
    }
}
