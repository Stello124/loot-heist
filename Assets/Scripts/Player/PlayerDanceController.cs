using UnityEngine;
using Unity.Netcode;

public class PlayerDanceController : NetworkBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [ServerRpc]
    public void TriggerDanceAtServerRpc(Vector3 position, string danceName)
    {
        TriggerDanceClientRpc(position, danceName);
    }

    [ClientRpc]
    private void TriggerDanceClientRpc(Vector3 position, string danceName)
    {
        if (!IsOwner) return;

        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
        animator.SetTrigger(danceName);
        Debug.Log("[Dance Triggered] " + danceName + " at position " + position + " by ClientID: " + OwnerClientId);
    }
}