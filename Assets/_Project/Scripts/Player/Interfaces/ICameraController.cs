/// <summary>
/// Interface for camera control interactions
/// </summary>
public interface ICameraController
{
    void Shake(float amount);
    void ResetCamera();
    void SetEnabled(bool enabled);
}
