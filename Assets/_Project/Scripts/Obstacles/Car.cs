using UnityEngine;

public class Car : MonoBehaviour
{
    [Tooltip("how fast the car moves")]
    public float speed = 5f;

    private void FixedUpdate()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
