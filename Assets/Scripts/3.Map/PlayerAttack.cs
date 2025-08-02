using UnityEngine;
using Unity.Netcode;

public class PlayerAttack : NetworkBehaviour
{
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public Transform attackOrigin;
    public AudioClip punchSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Sadece owner için input al
        if (!IsOwner) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        RaycastHit hit;
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out hit, attackRange, playerLayer))
        {
            GameObject hitPlayer = hit.collider.gameObject;

            if (hitPlayer.CompareTag("Player"))
            {
                if (BombManager.Instance.GetCurrentBombHolder() == this.gameObject)
                {
                    NetworkObject hitNetObj = hitPlayer.GetComponent<NetworkObject>();
                    if (hitNetObj != null)
                    {
                        Debug.Log($"{gameObject.name} passed bomb to {hitPlayer.name} with punch!");
                        
                        // Server'a bomba transferi isteği gönder
                        AttackTransferBombServerRpc(hitNetObj.OwnerClientId);
                        
                        // Ses efekti - sadece saldıran oyuncu duyar
                        if (punchSound != null && audioSource != null)
                            audioSource.PlayOneShot(punchSound);
                    }
                }
            }
        }
    }
    
    [ServerRpc]
    private void AttackTransferBombServerRpc(ulong targetClientId)
    {
        if (BombManager.Instance != null)
        {
            BombManager.Instance.TransferBombToClient(targetClientId);
        }
    }
}

