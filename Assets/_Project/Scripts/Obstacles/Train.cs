using UnityEngine;

public class Train : MonoBehaviour
{
    public float speed = 20f; // Trains are usually fast!

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}