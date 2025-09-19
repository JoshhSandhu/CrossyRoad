using UnityEngine;

public class Despawner : MonoBehaviour
{
    [Tooltip("reference to the player's transform.")]
    public Transform player;

    [Tooltip("dist from the player before removing")]
    [SerializeField] private float despawnDistance = 10f;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (transform.position.z < player.position.z - despawnDistance)
        {
            Destroy(gameObject);
        }
    }
}