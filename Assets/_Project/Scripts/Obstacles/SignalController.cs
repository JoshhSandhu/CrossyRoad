using System.Collections;
using UnityEngine;

public class SignalController : MonoBehaviour
{
    private Light signalLight;

    void Awake()
    {
        // Get the Light component from one of the children
        signalLight = GetComponentInChildren<Light>();
        signalLight.enabled = false; // Start with the light off
    }

    // A public method that the ObstacleSpawner can call to start the flashing
    public IEnumerator FlashWarning(float duration)
    {
        // Flash the light 5 times over the given duration
        for (int i = 0; i < 5; i++)
        {
            signalLight.enabled = true;
            yield return new WaitForSeconds(duration / 10);
            signalLight.enabled = false;
            yield return new WaitForSeconds(duration / 10);
        }
    }
}