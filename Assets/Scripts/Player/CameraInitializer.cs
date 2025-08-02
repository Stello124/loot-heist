using Controller;
using Unity.Netcode;
using UnityEngine;

public class CameraInitializer : NetworkBehaviour
{
    [SerializeField] private GameObject cameraPrefab;

    private void Start()
    {
        if (!IsOwner) return;

        // Kamerayý oluþtur
        GameObject cam = Instantiate(cameraPrefab);

        // Kameradaki ThirdPersonCamera bileþenine player referansý ver
        var tps = cam.GetComponent<ThirdPersonCamera>();
        tps.SetPlayer(transform);
    }
}
