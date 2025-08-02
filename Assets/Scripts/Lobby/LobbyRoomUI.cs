using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Services.Authentication;
using System;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;

public class LobbyRoomUI : MonoBehaviour
{

    [Header("Lobby Bilgileri")]
    [SerializeField] public TextMeshProUGUI lobbyNameText;
    [SerializeField] public TextMeshProUGUI lobbyCodeText;
    [SerializeField] public TextMeshProUGUI gameModeText;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject clientButton;
    [SerializeField] private GameObject readyButton;


    [Header("Start Button")]
    [SerializeField] private GameObject startGameButton;

    [Header("Oyuncu Listesi")]
    [SerializeField] public Transform playerListContainer;
    [SerializeField] public GameObject playerEntryPrefab;

    [Header("Güncelleme Input'ları")]
    [SerializeField] private TMP_InputField newLobbyNameInput;
    public string joinCode;
    

    private CurrentLobby _currentLobby;
    private string lobbyId;

    private Dictionary<string, GameObject> playerEntries = new();

    void Start()
    {
        _currentLobby = GameObject.Find("LobbyManager")?.GetComponent<CurrentLobby>();
        if (_currentLobby == null || _currentLobby.currentLobby == null) return;

        lobbyId = _currentLobby.currentLobby.Id;

        UpdateLobbyUI();
        InvokeRepeating(nameof(PollForLobbyUpdate), 2f, 2.5f);
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

            // Özellikle buton görünürlüğünü güncelle
            UpdateStartButtonVisibility();
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

            // Görsel güncelleme
            if (playerEntries.TryGetValue(AuthenticationService.Instance.PlayerId, out GameObject entry))
            {
                Transform readyIcon = entry.transform.Find("ReadyIcon");
                if (readyIcon != null)
                {
                    UnityEngine.UI.Image iconImage = readyIcon.GetComponent<UnityEngine.UI.Image>();
                    iconImage.color = newStatus == "1" ? Color.green : Color.red;
                }
            }

            // ⭐ Önemli: Ready durumu değişince buton görünürlüğünü güncelle
            UpdateStartButtonVisibility();

            // Eğer ready iptal edildiyse, tüm start butonlarını gizle
            if (newStatus == "0")
            {
                startGameButton.SetActive(false);
                if (clientButton != null) clientButton.SetActive(false);
                Debug.Log("🔄 Ready iptal edildi, start butonları gizlendi");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("❌ Hazır durumu güncellenemedi: " + e.Message);
        }
    }


    public RelayManager relayManager; // Inspector'dan bağla
    public TMP_InputField joinCodeInput;

    public async void HandleStartGameButtonClick()
    {
        Debug.Log("🎮 HOST START butonuna basıldı!");

        if (!AreAllPlayersReady())
        {
            Debug.LogWarning("🚫 Tüm oyuncular hazır değil. Oyun başlatılamaz.");
            return;
        }

        // Host butonunu devre dışı bırak
        startGameButton.SetActive(false);

        if (!NetworkManager.Singleton.IsListening)
        {
            Debug.Log("Relay allocation başlatılıyor...");

            try
            {
                // Relay allocation oluştur
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

                // Join kodunu al
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                if (joinCodeInput != null)
                    joinCodeInput.text = joinCode;

                Debug.Log($"Relay Join Code: {joinCode}");

                // Relay kodunu Lobby Data'ya ekle
                await LobbyData.Instance.SetRelayCodeToLobby(lobbyId, joinCode);

                // NetworkManager'a relay bilgilerini ver
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                Debug.Log("Relay bağlantısı kuruldu. Host başlatılıyor...");
                NetworkManager.Singleton.StartHost();

                // Sahne geçişi ve oyun başlatma
                StartCoroutine(WaitAndStartGame());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Relay bağlantısı başarısız: {ex.Message}");
                // Hata durumunda butonu geri aç
                startGameButton.SetActive(true);
            }
        }
        else
        {
            // Zaten host ise direkt oyunu başlat
            StartGameBasedOnMode();
        }
    }

    public RelayManager RelayManager; // Inspector'dan bağla


    public async void ClientStartGameButtonClick()
    {
        Debug.Log("🔄 CLIENT START butonuna basıldı!");

        // Client butonunu devre dışı bırak
        if (clientButton != null) clientButton.SetActive(false);

        Debug.Log("🔄 Client relay kodunu lobby'dan alıyor...");

        // Lobby'dan relay kodunu al
        string relayCodeFromLobby = await LobbyData.Instance.GetRelayCodeFromLobby(lobbyId);

        if (string.IsNullOrEmpty(relayCodeFromLobby))
        {
            Debug.LogError("❌ Lobby'da relay kodu bulunamadı. Host oyunu henüz başlatmamış olabilir.");
            // Hata durumunda butonu geri aç
            if (clientButton != null) clientButton.SetActive(true);
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCodeFromLobby);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            Debug.Log("✅ Client relay bağlantısı kuruldu.");
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Relay client bağlantısı başarısız: {ex.Message}");
            // Hata durumunda butonu geri aç
            if (clientButton != null) clientButton.SetActive(true);
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
        if (_currentLobby == null || _currentLobby.currentLobby == null ||
            startGameButton == null)
        {
            return;
        }

        bool isHost = _currentLobby.currentLobby.HostId == AuthenticationService.Instance.PlayerId;
        bool allPlayersReady = CheckAllPlayersReady();

        Debug.Log($"Durum - Host: {isHost}, Tümü Hazır: {allPlayersReady}");

        if (allPlayersReady)
        {
            // Herkes hazır - butonları göster
            if (isHost)
            {
                // Host için START butonu
                startGameButton.SetActive(true);

                // Host'un kendi ready butonunu gizle
                if (readyButton != null) readyButton.SetActive(false);

                // Host'un client butonunu gizle
                if (clientButton != null) clientButton.SetActive(false);

                Debug.Log("✅ Host için START tuşu aktif");
            }
            else
            {
                // Client için CLIENT START butonu
                startGameButton.SetActive(false);

                // Client'ın ready butonunu gizle
                if (readyButton != null) readyButton.SetActive(false);

                // Client'ın CLIENT START butonunu göster
                if (clientButton != null)
                {
                    clientButton.SetActive(true);
                    Debug.Log("✅ Client için CLIENT START tuşu aktif");
                }
            }
        }
        else
        {
            // Herkes hazır değil - sadece ready butonları görünsün
            startGameButton.SetActive(false);

            if (clientButton != null) clientButton.SetActive(false);

            // Ready butonu sadece kendi durumuna göre
            string localPlayerId = AuthenticationService.Instance.PlayerId;
            Player localPlayer = _currentLobby.currentLobby.Players.Find(p => p.Id == localPlayerId);

            if (localPlayer != null)
            {
                bool isLocalPlayerReady = localPlayer.Data.ContainsKey("Ready") && localPlayer.Data["Ready"].Value == "1";

                if (readyButton != null)
                {
                    readyButton.SetActive(!isLocalPlayerReady); // Hazır değilse göster
                    Debug.Log($"Ready tuşu durumu: {!isLocalPlayerReady} (oyuncu hazır: {isLocalPlayerReady})");
                }
            }
        }
    }

    private void HideAllClientButtons()
    {
        foreach (var entry in playerEntries.Values)
        {
            Transform clientBtn = entry.transform.Find("ClientButton");
            if (clientBtn != null)
            {
                clientBtn.gameObject.SetActive(false);
            }
        }
    }

    private void ShowClientButtonDelayed()
    {
        clientButton.SetActive(true);
    }

    bool CheckAllPlayersReady()
    {
        if (_currentLobby?.currentLobby?.Players == null)
            return false;

        foreach (var player in _currentLobby.currentLobby.Players)
        {
            if (!player.Data.ContainsKey("Ready") || player.Data["Ready"].Value != "1")
            {
                return false;
            }
        }
        return true;
    }

    public void OnHostStartPressed()
    {
        if (_currentLobby == null || _currentLobby.currentLobby == null) return;
        string hostId = _currentLobby.currentLobby.HostId;

        foreach (var player in _currentLobby.currentLobby.Players)
        {
            string playerId = player.Id;
            if (playerEntries.TryGetValue(playerId, out GameObject entry))
            {
                // 🔒 Ready tuşunu kapat (herkeste)
                Transform readyBtn = entry.transform.Find("ReadyButton");
                if (readyBtn != null)
                {
                    readyBtn.gameObject.SetActive(false);
                    Debug.Log($"🔒 Ready tuşu kapatıldı - Player: {playerId}");
                }

                // 🔧 Client tuşunu sadece HOST OLMAYANLARA göster
                if (playerId != hostId)
                {
                    Transform clientBtn = entry.transform.Find("ClientButton");
                    if (clientBtn != null)
                    {
                        StartCoroutine(ShowClientButtonDelayedForPlayer(clientBtn.gameObject, playerId));
                    }
                }
            }
        }

        // 🔒 Host'un Start tuşunu da kapat
        startGameButton.SetActive(false);
        Debug.Log("🎮 Host START bastı → Ready tuşları kapatıldı, Client tuşları 1sn sonra gelecek");
    }

    private IEnumerator ShowClientButtonDelayedForPlayer(GameObject clientBtn, string playerId)
    {
        yield return new WaitForSeconds(1f);
        if (clientBtn != null)
        {
            clientBtn.SetActive(true);
            Debug.Log($"✅ ClientButton aktif edildi - Player: {playerId}");
        }
    }


    private bool AreAllPlayersReady()
    {
        return CheckAllPlayersReady();
    }
}