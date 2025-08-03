using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Controller;

public class SpectateManager : NetworkBehaviour
{
    [Header("Spectate Settings")]
    public Camera spectateCamera; // Map üstündeki kamera
    public float spectateCameraHeight = 20f;
    public float spectateCameraDistance = 15f;
    
    [Header("UI")]
    public GameObject spectateUI;
    public TextMeshProUGUI spectateText;
    
    private NetworkVariable<bool> isSpectating = new NetworkVariable<bool>(false);
    private NetworkVariable<ulong> spectatingPlayerId = new NetworkVariable<ulong>(0);
    
    private Camera playerCamera;
    private PlayerCamera playerCameraScript;
    private GameObject currentSpectateTarget;
    private int currentSpectateIndex = 0;
    private List<GameObject> alivePlayers = new List<GameObject>();
    
    public override void OnNetworkSpawn()
    {
        // Sadece owner spectate sistemini başlatsın
        if (!IsOwner) return;
        
        // Oyuncu kamerasını bul
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            playerCameraScript = playerCamera.GetComponent<PlayerCamera>();
        }
        
        // Spectate kamerasını oluştur
        CreateSpectateCamera();
        
        // UI oluştur
        CreateSpectateUI();
        
        Debug.Log($"🎥 SpectateManager başlatıldı - Owner: {IsOwner}");
    }
    
    private void CreateSpectateCamera()
    {
        if (spectateCamera == null)
        {
            GameObject cameraObj = new GameObject("SpectateCamera");
            spectateCamera = cameraObj.AddComponent<Camera>();
            spectateCamera.tag = "MainCamera";
            
            // Map üstünde konumlandır
            cameraObj.transform.position = new Vector3(0, spectateCameraHeight, -spectateCameraDistance);
            cameraObj.transform.LookAt(Vector3.zero);
            
            // Audio listener'ı devre dışı bırak (çakışma olmasın)
            AudioListener audioListener = spectateCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
            
            spectateCamera.gameObject.SetActive(false);
        }
    }
    
    private void CreateSpectateUI()
    {
        if (spectateUI == null)
        {
            // Canvas oluştur
            GameObject canvasObj = new GameObject("SpectateCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Spectate panel
            GameObject panelObj = new GameObject("SpectatePanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.8f);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Spectate text
            GameObject textObj = new GameObject("SpectateText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            spectateText = textObj.AddComponent<TextMeshProUGUI>();
            spectateText.text = "SPECTATING";
            spectateText.color = Color.white;
            spectateText.fontSize = 24;
            spectateText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            spectateUI = panelObj;
            spectateUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Sadece owner input alsın
        if (!IsOwner) return;
        
        // Spectate modunda kontroller
        if (isSpectating.Value)
        {
            HandleSpectateInput();
            UpdateSpectateCamera();
        }
    }
    
    private void HandleSpectateInput()
    {
        // Q tuşu ile spectate kamerasına geç
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchToSpectateCamera();
        }
        
        // E tuşu ile oyuncu kamerasına geç
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchToPlayerCamera();
        }
        
        // Tab tuşu ile oyuncular arası geçiş
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchToNextPlayer();
        }
    }
    
    private void SwitchToSpectateCamera()
    {
        if (spectateCamera != null)
        {
            // Oyuncu kamerasını kapat
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
            
            // Spectate kamerasını aç
            spectateCamera.gameObject.SetActive(true);
            
            // UI güncelle
            if (spectateText != null)
            {
                spectateText.text = "SPECTATING - MAP VIEW (Q/E to switch, Tab to change player)";
            }
            
            Debug.Log("🎥 Spectate kamerasına geçildi");
        }
    }
    
    private void SwitchToPlayerCamera()
    {
        if (playerCamera != null)
        {
            // Spectate kamerasını kapat
            if (spectateCamera != null)
            {
                spectateCamera.gameObject.SetActive(false);
            }
            
            // Oyuncu kamerasını aç
            playerCamera.gameObject.SetActive(true);
            
            // UI güncelle
            if (spectateText != null && currentSpectateTarget != null)
            {
                spectateText.text = $"SPECTATING - {currentSpectateTarget.name} (Q/E to switch, Tab to change player)";
            }
            
            Debug.Log("🎥 Oyuncu kamerasına geçildi");
        }
    }
    
    private void SwitchToNextPlayer()
    {
        UpdateAlivePlayersList();
        
        if (alivePlayers.Count == 0) return;
        
        currentSpectateIndex = (currentSpectateIndex + 1) % alivePlayers.Count;
        currentSpectateTarget = alivePlayers[currentSpectateIndex];
        
        // Oyuncu kamerasını bu oyuncuya odakla
        if (playerCameraScript != null && currentSpectateTarget != null)
        {
            playerCameraScript.SetPlayer(currentSpectateTarget.transform);
        }
        
        // UI güncelle
        if (spectateText != null)
        {
            spectateText.text = $"SPECTATING - {currentSpectateTarget.name} (Q/E to switch, Tab to change player)";
        }
        
        Debug.Log($"🎥 Spectate hedefi değiştirildi: {currentSpectateTarget.name}");
    }
    
    private void UpdateSpectateCamera()
    {
        if (spectateCamera != null)
        {
            // Map üstündeki kamera pozisyonunu güncelle
            Vector3 centerPos = Vector3.zero;
            
            // Hayatta kalan oyuncuların merkezini bul
            UpdateAlivePlayersList();
            if (alivePlayers.Count > 0)
            {
                centerPos = alivePlayers.Aggregate(Vector3.zero, (sum, player) => sum + player.transform.position) / alivePlayers.Count;
            }
            
            spectateCamera.transform.position = centerPos + new Vector3(0, spectateCameraHeight, -spectateCameraDistance);
            spectateCamera.transform.LookAt(centerPos);
        }
    }
    
    private void UpdateAlivePlayersList()
    {
        alivePlayers.Clear();
        
        // Hayatta kalan oyuncuları bul
        var allPlayers = FindObjectsOfType<NetworkObject>()
            .Where(obj => obj.CompareTag("Player") && obj.IsSpawned)
            .Select(obj => obj.gameObject)
            .ToList();
        
        alivePlayers.AddRange(allPlayers);
    }
    
    [ClientRpc]
    public void StartSpectatingClientRpc(ulong playerId)
    {
        // Sadece local client spectate moduna geçsin
        if (NetworkManager.Singleton.LocalClientId != playerId) return;
        
        Debug.Log($"🎥 Spectate modu başlatılıyor - Local Client: {playerId}");
        isSpectating.Value = true;
        spectatingPlayerId.Value = playerId;
        
        // UI'ı göster
        if (spectateUI != null)
        {
            spectateUI.SetActive(true);
        }
        
        // İlk spectate hedefini ayarla
        UpdateAlivePlayersList();
        if (alivePlayers.Count > 0)
        {
            currentSpectateTarget = alivePlayers[0];
            if (playerCameraScript != null)
            {
                playerCameraScript.SetPlayer(currentSpectateTarget.transform);
            }
            
            if (spectateText != null)
            {
                spectateText.text = $"SPECTATING - {currentSpectateTarget.name} (Q/E to switch, Tab to change player)";
            }
        }
        
        Debug.Log("🎥 Spectate modu başladı!");
    }
    
    [ClientRpc]
    public void StopSpectatingClientRpc()
    {
        // Sadece spectate modundaki local client'ı durdur
        if (!isSpectating.Value) return;
        
        Debug.Log("🎥 Spectate modu sonlandırılıyor");
        isSpectating.Value = false;
        spectatingPlayerId.Value = 0;
        
        // UI'ı gizle
        if (spectateUI != null)
        {
            spectateUI.SetActive(false);
        }
        
        // Kameraları normale döndür
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }
        
        if (spectateCamera != null)
        {
            spectateCamera.gameObject.SetActive(false);
        }
        
        Debug.Log("🎥 Spectate modu sonlandı!");
    }
    
    // BombManager'dan çağrılacak
    public void OnPlayerDied(ulong deadPlayerId)
    {
        // Sadece ölen oyuncunun kendisi spectate moduna geçsin
        if (IsOwner && NetworkManager.Singleton.LocalClientId == deadPlayerId)
        {
            Debug.Log($"🎥 Oyuncu öldü, spectate modu başlatılıyor: {deadPlayerId}");
            StartSpectatingClientRpc(deadPlayerId);
        }
    }
    
    // Oyun bittiğinde çağrılacak
    public void OnGameEnded()
    {
        // Sadece spectate modundaki oyuncuları durdur
        if (isSpectating.Value)
        {
            Debug.Log("🎥 Oyun bitti, spectate modu sonlandırılıyor");
            StopSpectatingClientRpc();
        }
    }
} 