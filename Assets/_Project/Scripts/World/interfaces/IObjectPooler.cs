using UnityEngine;

public interface IObjectPooler
{
    void CreatePool(string poolName, GameObject prefab, int size);
    GameObject SpawnFromPool(string poolName, Vector3 position, Quaternion rotation);
}
