using Unity.Netcode;
using UnityEngine;

public class PositionSync : NetworkBehaviour
{
    public NetworkVariable<Vector3> syncedPosition = new(writePerm: NetworkVariableWritePermission.Owner);

    void Update()
    {
        if (IsOwner)
        {
            syncedPosition.Value = transform.position;
        }
        else
        {
            transform.position = syncedPosition.Value;
        }
    }
}