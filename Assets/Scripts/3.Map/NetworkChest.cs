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
        Debug.Log($"🎯 CHEST TRİGGER: {other.name} ({other.tag}) - IsServer: {IsServer}");
        
        // Player kontrol et
        if (!other.CompareTag("Player")) 
        {
            Debug.Log($"❌ Player tag'i değil: {other.tag}");
            return;
        }
        
        // NetworkObject kontrol et
        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) 
        {
            Debug.Log("❌ NetworkObject component yok!");
            return;
        }
        
        // Eğer server'daysa direkt işlem yap
        if (IsServer)
        {
            Debug.Log("✅ Server'da trigger - direkt işlem yapıyor");
            HandleChestPickup(playerNetObj.OwnerClientId);
        }
        // Eğer client'taysa ve bu oyuncunun own ettiği player'ıysa server'a RPC gönder
        else if (playerNetObj.IsOwner)
        {
            Debug.Log("✅ Client'ta trigger - Server'a RPC gönderiyor");
            RequestChestPickupServerRpc(playerNetObj.OwnerClientId);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestChestPickupServerRpc(ulong clientId)
    {
        Debug.Log($"🎯 Server RPC alındı! Client: {clientId}");
        HandleChestPickup(clientId);
    }
    
    private void HandleChestPickup(ulong clientId)
    {
        // Chest müsait değilse hiçbir şey yapma
        if (!isAvailable.Value) 
        {
            Debug.Log("❌ Chest müsait değil!");
            return;
        }
        
        Debug.Log($"✅📦 {chestType} chest alındı - Client: {clientId}");
        
        // Chest'i devre dışı bırak
        isAvailable.Value = false;
        
        // Oyuncuya boost ver
        ApplyBoostToPlayerClientRpc(clientId);
        
        // Pickup effect göster
        ShowPickupEffectClientRpc();
        
        // Respawn coroutine başlat
        StartCoroutine(RespawnAfterDelay());
    }
    
    [ClientRpc]
    private void ApplyBoostToPlayerClientRpc(ulong targetClientId)
    {
        Debug.Log($"🎯 ClientRPC alındı! Target: {targetClientId}, LocalClient: {NetworkManager.Singleton.LocalClientId}");
        
        // Sadece hedef client boost alsın
        if (NetworkManager.Singleton.LocalClientId != targetClientId) 
        {
            Debug.Log("❌ Bu client'a gönderilmemiş, ignore ediliyor");
            return;
        }
        
        Debug.Log($"✅ Bu client'a gönderilmiş! Player aranıyor...");
        
        // Oyuncuyu bul
        var networkObjects = FindObjectsOfType<NetworkObject>();
        Debug.Log($"🔍 {networkObjects.Length} NetworkObject bulundu");
        
        foreach (var netObj in networkObjects)
        {
            Debug.Log($"🔍 Kontrol: {netObj.name} - Owner: {netObj.OwnerClientId} - Tag: {netObj.tag}");
            
            if (netObj.OwnerClientId == targetClientId && netObj.CompareTag("Player"))
            {
                Debug.Log($"✅ Player bulundu: {netObj.name}");
                
                // CharacterMover'a direkt boost uygula (BoostReceiver'a gerek yok!)
                var characterMover = netObj.GetComponent<Controller.CharacterMover>();
                if (characterMover != null)
                {
                    Debug.Log($"✅ CharacterMover bulundu, boost uygulanıyor...");
                    ApplyBoostDirectly(characterMover, chestType);
                    break;
                }
                else
                {
                    Debug.LogWarning($"❌ {netObj.name}'de CharacterMover component yok!");
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
        float boostedJump = originalJump * 2f; // Mevcut zıplamanın 2 katı
        float boostDuration = 4f;
        
        Debug.Log($"🦘 Jump boost: {originalJump} → {boostedJump}");
        mover.SetJumpHeight(boostedJump);
        yield return new WaitForSeconds(boostDuration);
        mover.SetJumpHeight(originalJump);
        
        Debug.Log("🦘 Jump boost sona erdi");
    }
    
    private System.Collections.IEnumerator ApplySpeedBoostCoroutine(Controller.CharacterMover mover)
    {
        float originalSpeed = mover.GetRunSpeed();
        float boostedSpeed = originalSpeed * 2f; // Mevcut hızın 2 katı
        float boostDuration = 4f;
        
        Debug.Log($"⚡ Speed boost: {originalSpeed} → {boostedSpeed}");
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