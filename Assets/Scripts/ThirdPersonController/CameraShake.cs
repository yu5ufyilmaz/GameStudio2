using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    public CinemachineImpulseSource impulseSource;

    void Awake()
    {
        Instance = this;
    }

    public void ShakeCamera(float intensity)
    {
        var velocity = new Vector3(0, -05f, -1);
        velocity.Normalize();
        impulseSource.GenerateImpulse(velocity * intensity * 0.4f);
    }
}
