using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Server);

    public float moveSpeed = 3f;

    private void Start()
    {
        // Pozisyon deðiþtiðinde sadece localde görsel olarak güncelle
        Position.OnValueChanged += (oldPos, newPos) =>
        {
            transform.position = newPos;
        };
    }

    
    private void Update()
    {
        if (!IsOwner) return;

        Vector3 inputDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) inputDir.z += 1f;
        if (Input.GetKey(KeyCode.S)) inputDir.z -= 1f;
        if (Input.GetKey(KeyCode.A)) inputDir.x -= 1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x += 1f;

        if (inputDir != Vector3.zero)
        {
            // Time.deltaTime'ý client'ta deðil server'da uygula
            MoveRequestServerRpc(inputDir.normalized);
        }
    }

    [ServerRpc]
    private void MoveRequestServerRpc(Vector3 direction)
    {
        Position.Value += direction * moveSpeed * Time.fixedDeltaTime;
    }
}
