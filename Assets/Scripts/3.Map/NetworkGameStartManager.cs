using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkGameStartManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public GameObject introPanel;
    public TMP_Text introText;
    public TMP_Text countdownText;
    public TMP_Text playerStatusText;
    
    [Header("Settings")]
    public float waitForPlayersTime = 20f; // Oyuncuları bekleme süresi
    public float gameStartCountdown = 15f; // Oyun başlama geri sayımı
    public float playerTimeoutCheck = 6f; // Oyuncu timeout kontrolü
    public int expectedPlayerCount = 4; // Beklenen oyuncu sayısı
    
    private NetworkVariable<float> currentCountdown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isWaitingForPlayers = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> connectedPlayerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private Coroutine gameStartCoroutine;
    private bool gameStarted = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Network event'leri dinle
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            // BombManager artık kendi sistemini kullanıyor, bu UI devre dışı
            Debug.Log("🎮 NetworkGameStartManager: BombManager otomatik çalışıyor, UI sistemi devre dışı");
        }
        
        // UI güncellemelerini dinle
        currentCountdown.OnValueChanged += OnCountdownChanged;
        isWaitingForPlayers.OnValueChanged += OnWaitingStateChanged;
        connectedPlayerCount.OnValueChanged += OnPlayerCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        currentCountdown.OnValueChanged -= OnCountdownChanged;
        isWaitingForPlayers.OnValueChanged -= OnWaitingStateChanged;
        connectedPlayerCount.OnValueChanged -= OnPlayerCountChanged;
    }

    private void StartWaitingForPlayers()
    {
        if (!IsServer || gameStarted) return;
        
        Debug.Log("🎮 3.Map NetworkGameStartManager başladı - Oyuncular bekleniyor...");
        
        isWaitingForPlayers.Value = true;
        currentCountdown.Value = waitForPlayersTime;
        connectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        
        ShowIntroUIClientRpc();
        
        if (gameStartCoroutine != null) StopCoroutine(gameStartCoroutine);
        gameStartCoroutine = StartCoroutine(WaitingLoop());
    }
    
    [ClientRpc]
    private void ShowIntroUIClientRpc()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            
            if (introText != null)
                introText.text = "Bomba kimin elindeyse, 15 saniye içinde birine pasla yoksa patlarsın!";
                
            if (countdownText != null)
                countdownText.gameObject.SetActive(true);
                
            if (playerStatusText != null)
                playerStatusText.gameObject.SetActive(true);
        }
    }

    private IEnumerator WaitingLoop()
    {
        while (currentCountdown.Value > 0 && isWaitingForPlayers.Value && !gameStarted)
        {
            yield return new WaitForSeconds(1f);
            currentCountdown.Value -= 1f;
            
            UpdatePlayerStatusClientRpc(connectedPlayerCount.Value, expectedPlayerCount);
            
            // Tüm oyuncular bağlandıysa game start countdown'a geç
            if (connectedPlayerCount.Value >= expectedPlayerCount && currentCountdown.Value > gameStartCountdown)
            {
                Debug.Log("✅ Tüm oyuncular 3.map'te! Game start countdown başlıyor...");
                currentCountdown.Value = gameStartCountdown;
                isWaitingForPlayers.Value = false; // Waiting phase bitti
                StartGameCountdownClientRpc();
            }
        }
        
        // Countdown bitti - Oyunu başlat
        if (!gameStarted)
        {
            StartBombGameClientRpc();
        }
    }
    
    [ClientRpc]
    private void UpdatePlayerStatusClientRpc(int connected, int expected)
    {
        if (playerStatusText != null)
        {
            playerStatusText.text = $"Oyuncular: {connected}/{expected} 3.map'te";
        }
    }
    
    [ClientRpc]
    private void StartGameCountdownClientRpc()
    {
        if (introText != null)
            introText.text = "Tüm oyuncular hazır! Bomba oyunu başlıyor...";
    }
    
    [ClientRpc]
    private void StartBombGameClientRpc()
    {
        Debug.Log("🚀 Bomba oyunu başlıyor!");
        
        if (introPanel != null)
            StartCoroutine(HideIntroAndStartGame());
    }
    
    private IEnumerator HideIntroAndStartGame()
    {
        // Countdown göster: 3, 2, 1, BAŞLA!
        string[] countdownWords = { "3", "2", "1", "BAŞLA!" };
        
        foreach (string word in countdownWords)
        {
            if (countdownText != null)
                countdownText.text = word;
            yield return new WaitForSeconds(1f);
        }
        
        // Intro panel'i gizle
        if (introPanel != null)
            introPanel.SetActive(false);
        
        // Bomba oyununu başlat
        if (IsServer)
        {
            gameStarted = true;
            // BombManager artık otomatik başlıyor - manuel çağrı gerekmiyor
            Debug.Log("🎮 NetworkGameStartManager: BombManager otomatik başlatıldı");
            
            var gameFlowController = FindObjectOfType<GameFlowController>();
            if (gameFlowController != null)
            {
                gameFlowController.StartGame();
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        
        connectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log($"🌐 Oyuncu 3.map'e bağlandı: {clientId} (Toplam: {connectedPlayerCount.Value}/{expectedPlayerCount})");
        
        // Tüm oyuncular bağlandıysa countdown'u hızlandır
        if (connectedPlayerCount.Value >= expectedPlayerCount && isWaitingForPlayers.Value)
        {
            if (currentCountdown.Value > gameStartCountdown)
            {
                currentCountdown.Value = gameStartCountdown;
                isWaitingForPlayers.Value = false;
                StartGameCountdownClientRpc();
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        
        connectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log($"❌ Oyuncu 3.map'ten ayrıldı: {clientId} (Kalan: {connectedPlayerCount.Value}/{expectedPlayerCount})");
        
        // Timeout kontrolü başlat
        StartCoroutine(CheckPlayerTimeout(clientId));
    }
    
    private IEnumerator CheckPlayerTimeout(ulong disconnectedClientId)
    {
        yield return new WaitForSeconds(playerTimeoutCheck);
        
        // Hala bağlanmadıysa expected count'u azalt
        if (connectedPlayerCount.Value < expectedPlayerCount)
        {
            expectedPlayerCount = Mathf.Max(1, connectedPlayerCount.Value); // En az 1 oyuncu
            Debug.Log($"⏰ Oyuncu timeout - Yeni beklenen oyuncu sayısı: {expectedPlayerCount}");
            
            UpdatePlayerStatusClientRpc(connectedPlayerCount.Value, expectedPlayerCount);
        }
    }

    private void OnCountdownChanged(float previousValue, float newValue)
    {
        if (countdownText != null)
        {
            if (isWaitingForPlayers.Value)
            {
                countdownText.text = $"Oyuncular bekleniyor...\n{Mathf.Ceil(newValue)}";
            }
            else
            {
                countdownText.text = $"Bomba Oyunu Başlıyor!\n{Mathf.Ceil(newValue)}";
            }
        }
    }
    
    private void OnWaitingStateChanged(bool previousValue, bool newValue)
    {
        // UI state değişiklikleri burada yapılabilir
    }
    
    private void OnPlayerCountChanged(int previousValue, int newValue)
    {
        // Player count değişiklikleri burada yapılabilir
    }
    
    // Dışarıdan countdown'u iptal etmek için
    public void ForceStartGame()
    {
        if (!IsServer || gameStarted) return;
        
        Debug.Log("🚀 Oyun zorla başlatıldı!");
        gameStarted = true;
        
        if (gameStartCoroutine != null)
        {
            StopCoroutine(gameStartCoroutine);
            gameStartCoroutine = null;
        }
        
        StartBombGameClientRpc();
    }
}