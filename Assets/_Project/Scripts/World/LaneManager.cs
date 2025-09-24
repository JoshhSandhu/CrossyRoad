using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this script manages the queue of active lanes in the scene
/// and handles the lifecycle => removal and addition of lanes in the scene
/// </summary>

public class LaneManager : MonoBehaviour, ILaneManager
{
    //this is the max number of lanes to maintain in a scene
    [SerializeField] private int maxLanesInScene = 30;

    //tracking the saned lanes in order of their spawns
    private Queue<GameObject> activeLanes = new Queue<GameObject>();

    //adds a new lane to the active lanes queue
    public void AddLane(GameObject lane)
    {
        activeLanes.Enqueue(lane);
    }

    //removes the oldest lane from the queue and deactivates it
    public void RemoveOldestLane()
    {
        if (activeLanes.Count > 0)
        {
            GameObject oldLane = activeLanes.Dequeue();
            oldLane.SetActive(false);
        }
    }

    //checks if the lane should be removed or not based on the max lane count
    public bool ShouldRemoveLane()
    {
        //true if the lane needs to be removed
        return activeLanes.Count > maxLanesInScene; 
    }
}
