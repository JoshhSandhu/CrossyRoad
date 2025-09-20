using System.Collections;
using UnityEngine;

public class SignalController : MonoBehaviour
{
    private Light signalLight;

    void Awake()
    {
        signalLight = GetComponentInChildren<Light>();
        if (signalLight != null)
        {
            signalLight.enabled = false;
        }
    }

    public IEnumerator FlashWarning(float duration)
    {
        if (signalLight == null) yield break;

        for (int i = 0; i < 5; i++)
        {
            signalLight.enabled = true;
            yield return new WaitForSeconds(duration / 10);
            signalLight.enabled = false;
            yield return new WaitForSeconds(duration / 10);
        }
    }
}