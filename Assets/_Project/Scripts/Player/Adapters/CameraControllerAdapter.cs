using UnityEngine;

/// <summary>
/// Concrete implementation of ICameraController using CamFollow
/// </summary>
public class CameraControllerAdapter : ICameraController
{
    private readonly CamFollow cameraFollow;

    public CameraControllerAdapter(CamFollow cameraFollow)
    {
        this.cameraFollow = cameraFollow;
    }

    public void Shake(float amount)
    {
        if (cameraFollow != null)
        {
            cameraFollow.Shake(amount);
        }
    }

    public void ResetCamera()
    {
        if (cameraFollow != null)
        {
            cameraFollow.ResetCamera();
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (cameraFollow != null)
        {
            cameraFollow.enabled = enabled;
        }
    }
}
