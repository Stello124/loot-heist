using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Services.Authentication;

public class LobbyRoomUI : MonoBehaviour
{
    [Header("Lobby Bilgileri")]
    [SerializeField] public TextMeshProUGUI lobbyNameText;
    [SerializeField] public TextMeshProUGUI lobbyCodeText;
    [SerializeField] public TextMeshProUGUI gameModeText;

    [Header("Start Button")]
    [SerializeField] private GameObject startGameButton;

    [Header("Oyuncu Listesi")]
    [SerializeField] public Transform playerListContainer;
    [SerializeField] public GameObject playerEntryPrefab;

    [Header("Güncelleme Input'ları")]
    [SerializeField] private TMP_InputField newLobbyNameInput;

    private CurrentLobby _currentLobby;
    private string lobbyId;

    private Dictionary<string, GameObject> playerEntries = new();

    void Start()
    {
        _currentLobby = GameObject.Find("LobbyManager")?.GetComponent<CurrentLobby>();
        if (_currentLobby == null || _currentLobby.currentLobby == null) return;

        lobbyId = _currentLobby.currentLobby.Id;

        UpdateLobbyUI();
        InvokeRepeating(nameof(PollForLobbyUpdate), 1.5f, 2f);
        UpdateStartButtonVisibility();
    }

    private void OnEnable()
    {
        StartCoroutine(DelayedSceneManagerBind());
    }

    private IEnumerator DelayedSceneManagerBind()
    {
        float timeout = 2f;
        float timer = 0f;

        while (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogWarning("❌ SceneManager bağlanamadı (timeout). OnEnable iptal.");
                yield break;
            }
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        Debug.Log("✅ SceneManager event'ine başarıyla abone olundu.");
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
            }
            else
            {
                Debug.LogWarning("OnDisable sırasında SceneManager null, unsubscribe yapılamadı.");
            }

            // Event delegate olduğu için null kontrolü yapılmaz
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
        else
        {
            Debug.LogWarning("OnDisable sırasında NetworkManager.Singleton null, unsubscribe yapılamadı.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"🔗 Client bağlandı: {clientId}");
        // Gerekirse burada spawn veya sync işlemi tetiklenebilir
    }


    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log($"Client {clientId} sahne yüklemesini tamamladı: {sceneName}");

        if (NetworkManager.Singleton.IsHost)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        GameObject playerPrefab = Resources.Load<GameObject>("PlayerNetworkPrefab");
        if (playerPrefab == null)
        {
            Debug.LogError("❌ Player prefab bulunamadı! Resources klasöründe 'PlayerNetworkPrefab' adında prefab olmalı.");
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab);
        NetworkObject networkObj = playerInstance.GetComponent<NetworkObject>();

        if (networkObj == null)
        {
            Debug.LogError("❌ NetworkObject component eksik!");
            return;
        }

        networkObj.SpawnWithOwnership(clientId);
        Debug.Log($"✅ Oyuncu spawn edildi: {clientId}");
    }


    public void UpdateLobbyUI()
    {
        if (_currentLobby == null || _currentLobby.currentLobby == null) return;

        Lobby lobby = _currentLobby.currentLobby;

        lobbyNameText.text = lobby.Name;
        lobbyCodeText.text = lobby.LobbyCode;
        gameModeText.text = lobby.Data["GameMode"].Value;

        ClearPlayerList();
        foreach (Player player in lobby.Players)
        {
            AddPlayerEntry(player);
        }

        UpdateStartButtonVisibility();
    }

    public void UpdateLobbyUIFromLobby(Lobby lobby)
    {
        if (lobby == null) return;

        lobbyNameText.text = lobby.Name;
        lobbyCodeText.text = lobby.LobbyCode;
        gameModeText.text = lobby.Data["GameMode"].Value;

        ClearPlayerList();
        foreach (Player player in lobby.Players)
        {
            AddPlayerEntry(player);
        }

        UpdateStartButtonVisibility();
    }

    void AddPlayerEntry(Player player)
    {
        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
        playerEntries[player.Id] = entry;

        string playerName = player.Data.ContainsKey("PlayerName") ? player.Data["PlayerName"].Value : "İsimsiz";
        bool isHost = _currentLobby.currentLobby.HostId == player.Id;
        bool isReady = player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "1";

        TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
        nameText.text = playerName;

        Transform hostIcon = entry.transform.Find("HostIcon");
        if (hostIcon != null) hostIcon.gameObject.SetActive(isHost);

        Transform readyIcon = entry.transform.Find("ReadyIcon");
        if (readyIcon != null)
        {
            UnityEngine.UI.Image iconImage = readyIcon.GetComponent<UnityEngine.UI.Image>();
            iconImage.color = isReady ? Color.green : Color.red;
            iconImage.preserveAspect = true;
        }
    }

    void ClearPlayerList()
    {
        List<GameObject> toDestroy = new();

        foreach (Transform child in playerListContainer)
        {
            if (child != null && child.gameObject != null)
            {
                toDestroy.Add(child.gameObject);
            }
        }

        foreach (var go in toDestroy)
        {
            Destroy(go);
        }

        playerEntries.Clear();
    }

    async void PollForLobbyUpdate()
    {
        try
        {
            _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
            UpdateLobbyUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Lobby update failed: {e.Message}");
        }
    }

    public async void ChangeLobbyName()
    {
        string newName = newLobbyNameInput.text;
        if (string.IsNullOrEmpty(newName)) return;

        try
        {
            var options = new UpdateLobbyOptions { Name = newName };
            _currentLobby.currentLobby = await Lobbies.Instance.UpdateLobbyAsync(lobbyId, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void ToggleReadyStatus()
    {
        if (_currentLobby == null || _currentLobby.currentLobby == null) return;

        Player localPlayer = _currentLobby.currentLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);
        if (localPlayer == null) return;

        string currentStatus = localPlayer.Data.ContainsKey("Ready") ? localPlayer.Data["Ready"].Value : "0";
        string newStatus = currentStatus == "1" ? "0" : "1";

        var options = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newStatus) }
            }
        };

        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId, options);
            Debug.Log($"✅ Ready durumu güncellendi: {newStatus}");

            if (playerEntries.TryGetValue(AuthenticationService.Instance.PlayerId, out GameObject entry))
            {
                Transform readyIcon = entry.transform.Find("ReadyIcon");
                if (readyIcon != null)
                {
                    UnityEngine.UI.Image iconImage = readyIcon.GetComponent<UnityEngine.UI.Image>();
                    iconImage.color = newStatus == "1" ? Color.green : Color.red;
                }
            }

            UpdateLobbyUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("❌ Hazır durumu güncellenemedi: " + e.Message);
        }
    }

    public void HandleStartGameButtonClick()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Host relay bağlantısını kurmuş ve tüm oyuncular hazırsa oyunu başlatır
            if (!AreAllPlayersReady())
            {
                Debug.LogWarning("🚫 Tüm oyuncular hazır değil. Oyun başlatılamaz.");
                return;
            }

            Debug.Log("✅ Host oyunu başlatıyor...");
            StartGameBasedOnMode(); // Sahne geçişi burada yapılmalı
        }
        else
        {
            // Client sadece hosta bağlanır, host başlatmaz
            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.Log("🔗 Client olarak hosta bağlanılıyor...");
                NetworkManager.Singleton.StartClient(); // Relay üzerinden bağlanmalı
            }

            Debug.Log("🕒 Client, hostun sahne geçişini bekliyor...");
            // Client sahne geçişini host'tan alacak, kendi geçmeyecek
        }
    }

    private IEnumerator WaitAndStartGame()
    {
        float timeout = 1.5f;
        float timer = 0f;

        while (!NetworkManager.Singleton.IsHost && timer < timeout)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            if (!AreAllPlayersReady())
            {
                Debug.LogWarning("🚫 Tüm oyuncular hazır değil. Oyun başlatılamaz.");
                yield break;
            }

            StartGameBasedOnMode();
        }
        else
        {
            Debug.LogError("❌ Host olunamadı, oyun başlatılamıyor.");
        }
    }

    private void StartGameBasedOnMode()
    {
        string selectedMode = gameModeText.text;
        Debug.Log("✅ Oyun başlatılıyor. Mod: " + selectedMode);

        if (selectedMode == "Turnuva")
        {
            Debug.Log("🏁 Turnuva modu seçildi. Turnuva başlatılıyor...");
            TournamentManager.Instance.TurnuvayaBasla();
            return;
        }

        string sceneName = "DeneyK2";
        if (selectedMode == "Yarış" || selectedMode == "Bomba")
        {
            sceneName = "DeneyK2";
        }

        Debug.Log("🌍 Sahne yükleniyor: " + sceneName);
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    void UpdateStartButtonVisibility()
    {
        if (_currentLobby == null || _currentLobby.currentLobby == null || startGameButton == null) return;

        startGameButton.SetActive(true); // 🔧 Tüm oyuncularda buton aktif
    }

    private bool AreAllPlayersReady()
    {
        foreach (var player in _currentLobby.currentLobby.Players)
        {
            if (!player.Data.ContainsKey("Ready") || player.Data["Ready"].Value != "1")
            {
                return false;
            }
        }

        return true;
    }
}