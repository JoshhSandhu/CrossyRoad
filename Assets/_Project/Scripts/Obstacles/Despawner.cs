using UnityEngine;

public class Despawner : MonoBehaviour
{
    [Tooltip("reference to the player's transform.")]
    private Transform player;

    [Tooltip("dist behind the player before removing")]
    [SerializeField] private float despawnDistanceZ = 5f;

    [Tooltip("distance from the side of the player before despawning")]
    [SerializeField] private float despawnDistanceX = 35f;

    void Start()
    {
        //if the player transform is not assigned then assign it automatically
        if(GameManager.Instance != null)
        {
            player = GameManager.Instance.playerTransform;
        }
    }

    void Update()
    {
        //if the player doesn't have a ref, we can't do anything
        if (player == null) { return; }

        //check is the objects is too far behind or to far to the side
        if (transform.position.z < player.position.z - despawnDistanceZ || Mathf.Abs(transform.position.x) > despawnDistanceX)
        {
            Destroy(gameObject);
        }
    }
}