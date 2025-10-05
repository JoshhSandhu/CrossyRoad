using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// this file handels the spawning of decorations on the lanes
/// manages different decorations types
/// </summary>

public class DecorationSpawner : MonoBehaviour, IDecorationSpawner
{
    [Header("Path Validation settings")]
    [Tooltip("Min lily pad spacing")] 
    [SerializeField] private float minLilyPadSpace = 2.0f;
    [Tooltip("min clear space around lily")]
    [SerializeField] private float lilyPadClearRad = 1.5f;
    [Tooltip("max gap btw lily pads")]
    [SerializeField] private float maxLilyPadSpace = 4.0f;
    [Tooltip("the number of lily pads in a lane")]
    [SerializeField] private int targetLilyPadCount = 3;

    private LaneType previousLaneType;      //tracks previous lane type
    private int lastLilyPadX = 0;           //stores the last pos of the lily pad
    private List<int> currentLilyPadPos = new List<int>();

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
    //and ensures solvable paths
    private void SpawnPlatformDecorations(GameObject lane, GameObject decorationPrefab)
    {
        //storing the lily pad positions
        List<int> lilyPadPos = new List<int>();

        //determine number of lily pads
        int lilyPadCount = Random.Range(2, targetLilyPadCount + 1);

        int laneWidth = 10;
        int laneStart = -5;
        int laneEnd = 5;

        if (lilyPadCount == 2)
        {
            lilyPadPos.Add(laneStart + Random.Range(1, 3)); 
            lilyPadPos.Add(laneEnd - Random.Range(1, 3));   
        }
        else if (lilyPadCount == 3)
        {
            lilyPadPos.Add(laneStart + Random.Range(1, 2)); 
            lilyPadPos.Add(Random.Range(-1, 2));            
            lilyPadPos.Add(laneEnd - Random.Range(1, 2));   
        }

        lilyPadPos.Sort();

        if (!PathValidator.ValidateLilyPadPath(lilyPadPos, maxLilyPadSpace, laneWidth))
        {
            Debug.LogWarning("Generated lily pad path failed validation, regenerating...");
            lilyPadPos = PathValidator.GenerateSolvableLilyPadPath(laneWidth, minLilyPadSpace, maxLilyPadSpace, maxLilyPadSpace);
        }

        foreach (int xPos in lilyPadPos)
        {
            Vector3 decorationPos = new Vector3(xPos, lane.transform.position.y + 0.05f, lane.transform.position.z);
            GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
            newDecoration.transform.SetParent(lane.transform);
        }

        if (lilyPadPos.Count > 0)
        {
            lastLilyPadX = lilyPadPos[lilyPadPos.Count - 1];
        }
        StoreLilyPadPositions(lilyPadPos);
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
            if (Random.value < 0.75f && !IsPositionBlockingLilyPadPath(x))
            {
                GameObject treeObj = Instantiate(decorationPrefab, new Vector3(x, lane.transform.position.y + 0.15f, lane.transform.position.z), Quaternion.identity);
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

            int attempts = 0;
            const int maxAttempts = 10;

            //tying to find a pos that isn't blocking the lily pad
            while(IsPositionBlockingLilyPadPath(randX) && attempts < maxAttempts)
            {
                randX = Random.Range(-5, 5);
                attempts++;
            }
            if(attempts < maxAttempts)
            {
                Vector3 decorationPos = new Vector3(randX, lane.transform.position.y + 0.5f, lane.transform.position.z);
                GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
                newDecoration.transform.SetParent(lane.transform);
            }
        }
    }

    //spawns decorations randomly with random pos and collision avoidance
    private void SpawnNormalDecorations(GameObject lane, GameObject decorationPrefab, List<int> usedXPositions)
    {
        int decorationCount = Random.Range(0, 3);

        for (int i = 0; i < decorationCount; i++)
        {
            int randX = Random.Range(-5, 5);
            int attempts = 0;
            const int maxAttempts = 20;

            while ((usedXPositions.Exists(x => Mathf.Abs(x - randX) < 2) || IsPositionBlockingLilyPadPath(randX)) && attempts < maxAttempts)
            {
                randX = Random.Range(-5, 5);
                attempts++;
            }

            if (attempts < maxAttempts) 
            {
                usedXPositions.Add(randX);
                Vector3 decorationPos = new Vector3(randX, lane.transform.position.y + 0.15f, lane.transform.position.z);
                GameObject newDecoration = Instantiate(decorationPrefab, decorationPos, Quaternion.identity);
                newDecoration.transform.SetParent(lane.transform);
            }
        }
    }

    //stores lily pad positions for path validation
    private void StoreLilyPadPositions(List<int> lilyPadPositions)
    {
        currentLilyPadPos.Clear();
        currentLilyPadPos.AddRange(lilyPadPositions);
    }
    //checks if a position would block lily pad paths
    private bool IsPositionBlockingLilyPadPath(int xPosition)
    {
        return PathValidator.WouldBlockLilyPadPath(xPosition, currentLilyPadPos, lilyPadClearRad);
    }
}
