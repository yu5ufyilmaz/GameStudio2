using Cinemachine;
using UnityEngine;

public class CameraTargetSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine sanal kamerası
    public Transform headTarget; // Yeni takip hedefi
    public Transform playerTarget; // Yeni bakış hedefi

    // Bu metodu çağırarak hedefleri değiştirebilirsiniz
    public void SwitchTargets()
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = headTarget; // Yeni takip hedefini ayarla
            virtualCamera.LookAt = playerTarget; // Yeni bakış hedefini ayarla
        }
    }

    public void FixTarget()
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = playerTarget; // Yeni takip hedefini ayarla
            virtualCamera.LookAt = headTarget; // Yeni bakış hedefini ayarla
        }
    }
}
