using UnityEngine;

[CreateAssetMenu(fileName = "NewLaneType", menuName = "Crossy/Lane")] //this way we can create instances of this asset
public class LaneType : ScriptableObject
{
    [Tooltip("Give the lane a name")]
    public string laneName;

    [Tooltip("The prefab that will be spawned for this lane")]
    public GameObject lanePrefab;
}
