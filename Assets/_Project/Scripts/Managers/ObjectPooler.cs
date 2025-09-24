using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    //singleton instance of the object pooler
    public static ObjectPooler Instance { get; private set; }

    //to hold all our obj pools
    public Dictionary<string, Queue<GameObject>> poolDict;

    private void Awake()
    {
        //singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
        }
        else
        {
            Instance = this;
            poolDict = new Dictionary<string, Queue<GameObject>>();
        }
    }
    
    public void CreatePool(string tag, GameObject prefab, int size)
    {
        Debug.Log($"ObjectPooler.CreatePool called: tag={tag}, prefab={prefab?.name}, size={size}");
        if (poolDict.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " already exists.");
            return;
        }

        Queue<GameObject> objpool = new Queue<GameObject>();
        GameObject poolparent = new GameObject(tag + " Pool"); //craeting a parent to keep it organized in the hierarchy

        for ( int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, poolparent.transform);
            obj.SetActive(false); //in the start the obj is inactive
            objpool.Enqueue(obj); //adding the obj to the queue
        }

        poolDict.Add(tag, objpool); //adding the queue to the dictionary
        Debug.Log($"Successfully created pool '{tag}' with {size} objects");
    }

    public GameObject SpawnFromPool(string tag, Vector3 pos, Quaternion rot)
    {
        if (!poolDict.ContainsKey(tag))
        {
            Debug.LogError($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objtoSpawn = poolDict[tag].Dequeue(); //getting the first obj from the queue

        objtoSpawn.SetActive(true);
        objtoSpawn.transform.position = pos;
        objtoSpawn.transform.rotation = rot;

        //adding it back to the queue
        poolDict[tag].Enqueue(objtoSpawn);

        return objtoSpawn;

    }
}
