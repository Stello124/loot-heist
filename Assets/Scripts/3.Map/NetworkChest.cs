using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkChest : NetworkBehaviour
{
    [Header("Chest Settings")]
    public string chestType = "speed"; // "speed" or "jump"
    public float respawnTime = 3f;
    public AudioClip pickupSound;
    
    [Header("Visual Settings")]
    public GameObject chestModel;
    public GameObject pickupEffect;
    
    private NetworkVariable<bool> isAvailable = new NetworkVariable<bool>(
        true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private AudioSource audioSource;
    private Collider chestCollider;
    private Renderer chestRenderer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        chestCollider = GetComponent<Collider>();
        chestRenderer = GetComponent<Renderer>();
        
        // NetworkVariable değişikliklerini dinle
        isAvailable.OnValueChanged += OnAvailabilityChanged;
        
        // İlk durum ayarla
        UpdateVisualState(isAvailable.Value);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece server'da çalışır
        if (!IsServer) return;
        
        // Chest müsait değilse hiçbir şey yapma
        if (!isAvailable.Value) return;
        
        // Player kontrol et
        if (!other.CompareTag("Player")) return;
        
        // NetworkObject kontrol et
        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) return;
        
        Debug.Log($"📦 {chestType} chest alındı - Client: {playerNetObj.OwnerClientId}");
        
        // Chest'i devre dışı bırak
        isAvailable.Value = false;
        
        // Oyuncuya boost ver
        ApplyBoostToPlayerClientRpc(playerNetObj.OwnerClientId);
        
        // Pickup effect göster
        ShowPickupEffectClientRpc();
        
        // Respawn coroutine başlat
        StartCoroutine(RespawnAfterDelay());
    }
    
    [ClientRpc]
    private void ApplyBoostToPlayerClientRpc(ulong targetClientId)
    {
        // Sadece hedef client boost alsın
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        
        // Oyuncuyu bul
        var networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (var netObj in networkObjects)
        {
            if (netObj.OwnerClientId == targetClientId && netObj.CompareTag("Player"))
            {
                // CharacterMover'a direkt boost uygula (BoostReceiver'a gerek yok!)
                var characterMover = netObj.GetComponent<Controller.CharacterMover>();
                if (characterMover != null)
                {
                    ApplyBoostDirectly(characterMover, chestType);
                    break;
                }
            }
        }
    }
    
    private void ApplyBoostDirectly(Controller.CharacterMover mover, string boostType)
    {
        if (boostType == "jump")
        {
            Debug.Log("🦘 Jump boost uygulandı!");
            StartCoroutine(ApplyJumpBoostCoroutine(mover));
        }
        else if (boostType == "speed")
        {
            Debug.Log("⚡ Speed boost uygulandı!");
            StartCoroutine(ApplySpeedBoostCoroutine(mover));
        }
    }
    
    private System.Collections.IEnumerator ApplyJumpBoostCoroutine(Controller.CharacterMover mover)
    {
        float originalJump = mover.GetJumpHeight();
        float boostedJump = 7f;
        float boostDuration = 4f;
        
        mover.SetJumpHeight(boostedJump);
        yield return new WaitForSeconds(boostDuration);
        mover.SetJumpHeight(originalJump);
        
        Debug.Log("🦘 Jump boost sona erdi");
    }
    
    private System.Collections.IEnumerator ApplySpeedBoostCoroutine(Controller.CharacterMover mover)
    {
        float originalSpeed = mover.GetRunSpeed();
        float boostedSpeed = 7f;
        float boostDuration = 4f;
        
        mover.SetRunSpeed(boostedSpeed);
        yield return new WaitForSeconds(boostDuration);
        mover.SetRunSpeed(originalSpeed);
        
        Debug.Log("⚡ Speed boost sona erdi");
    }
    
    [ClientRpc]
    private void ShowPickupEffectClientRpc()
    {
        // Ses efekti
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Pickup effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }
    }
    
    private void OnAvailabilityChanged(bool oldValue, bool newValue)
    {
        UpdateVisualState(newValue);
    }
    
    private void UpdateVisualState(bool available)
    {
        if (chestModel != null)
        {
            chestModel.SetActive(available);
        }
        
        if (chestCollider != null)
        {
            chestCollider.enabled = available;
        }
        
        if (chestRenderer != null)
        {
            chestRenderer.enabled = available;
        }
    }
    
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);
        
        if (IsServer)
        {
            isAvailable.Value = true;
            Debug.Log($"✨ {chestType} chest respawn oldu!");
        }
    }
    
    void OnDestroy()
    {
        if (isAvailable != null)
        {
            isAvailable.OnValueChanged -= OnAvailabilityChanged;
        }
    }
}