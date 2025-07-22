using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using Unity.Netcode;

public class LobbyRoomUI : MonoBehaviour
{
    [Header("Lobby Bilgileri")]
    [SerializeField] public TextMeshProUGUI lobbyNameText;
    [SerializeField] public TextMeshProUGUI lobbyCodeText;
    [SerializeField] public TextMeshProUGUI gameModeText;

    [Header("Oyuncu Listesi")]
    [SerializeField] public Transform playerListContainer;
    [SerializeField] public GameObject playerEntryPrefab;

    [Header("Güncelleme Input'ları")]
    [SerializeField] private TMP_InputField newLobbyNameInput;

    [Header("Sahne Ayarları")]
    //[SerializeField] private string gameplaySceneName = "Gameplay"; // sahne adını dilediğin gibi ver

    private CurrentLobby _currentLobby;
    private string lobbyId;

    void Start()
    {
        _currentLobby = GameObject.Find("LobbyManager")?.GetComponent<CurrentLobby>();
        if (_currentLobby == null || _currentLobby.currentLobby == null) return;

        lobbyId = _currentLobby.currentLobby.Id;

        // 🔥 Panel verisini hemen doldur
        UpdateLobbyUI();

        // 🔄 Verileri düzenli olarak güncelle
        InvokeRepeating(nameof(PollForLobbyUpdate), 2f, 3f);
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
    }

    void AddPlayerEntry(Player player)
    {
        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);

        string playerName = player.Data.ContainsKey("PlayerName") ? player.Data["PlayerName"].Value : "İsimsiz";

        TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
        nameText.text = playerName;

        bool isHost = _currentLobby != null && _currentLobby.currentLobby != null &&
                      player.Id == _currentLobby.currentLobby.HostId;
        Transform icon = entry.transform.Find("HostIcon");
        if (icon != null) icon.gameObject.SetActive(isHost);
    }

    void ClearPlayerList()
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    async void PollForLobbyUpdate()
    {
        try
        {
            _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
            UpdateLobbyUI(); // 🌟 Bu satır çok kritik
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

    public void HandleStartGameButtonClick()
    {
        NetworkManager.Singleton.StartHost(); // 🔥 Host başlatılır

        SceneManager.LoadScene("DeneyK2"); // 🎯 geçilecek sahnenin adı
    }
}