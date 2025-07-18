using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class JoinLobby : MonoBehaviour
{
    [Header("Giriş")]
    [SerializeField] private TMP_InputField inputField;

    [Header("Panel Geçiş")]
    [SerializeField] private GameObject lobbyRoomPanel;
    [SerializeField] private GameObject lobbySetupPanel;
    [SerializeField] private LobbyRoomUI lobbyRoomUI;

    private CurrentLobby currentLobby;

    void Start()
    {
        currentLobby = GetComponent<CurrentLobby>();
    }

    public async void JoinLobbyWithLobbyCode()
    {
        string code = inputField.text;

        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "8") }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);

            currentLobby.currentLobby = lobby;
            DontDestroyOnLoad(this);

            Debug.Log($"Joined Lobby With Code = {code}");
            LobbyStatic.LogPlayersInLobby(lobby);

            // Panel geçişi
            lobbySetupPanel.SetActive(false);
            lobbyRoomPanel.SetActive(true);

            // Doğrudan lobby verisini UI'a gönder
            lobbyRoomUI.UpdateLobbyUIFromLobby(lobby); // 🔧 Yeni method üzerinden güncellenir
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyWithLobbyId(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "8") }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

            currentLobby.currentLobby = lobby;
            DontDestroyOnLoad(this);

            Debug.Log($"Joined Lobby With Id = {lobbyId}");
            LobbyStatic.LogPlayersInLobby(lobby);

            
            lobbySetupPanel.SetActive(false);
            lobbyRoomPanel.SetActive(true);

            lobbyRoomUI.UpdateLobbyUIFromLobby(lobby); 
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoinMethod()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            currentLobby.currentLobby = lobby;
            DontDestroyOnLoad(this);

            Debug.Log($"Joined Lobby With Quick Join = {lobby.Id}");
            LobbyStatic.LogPlayersInLobby(lobby);

            // Panel geçişi
            lobbySetupPanel.SetActive(false);
            lobbyRoomPanel.SetActive(true);

            lobbyRoomUI.UpdateLobbyUIFromLobby(lobby); // 🔧 Yeni method
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}