using UnityEngine;

public class Log : MonoBehaviour
{
    public float speed = 3f;

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}