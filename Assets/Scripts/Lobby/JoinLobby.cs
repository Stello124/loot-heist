using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinLobby : MonoBehaviour
{
    [Header("Giriş")]
    [SerializeField] private TMP_InputField inputField;

    [Header("Panel Geçiş")]
    [SerializeField] private GameObject lobbySetupPanel;

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
                        {
                            "PlayerName",
                            new PlayerDataObject(
                                PlayerDataObject.VisibilityOptions.Public,
                                StartupManager.PlayerName
                            )
                        }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);

            currentLobby.currentLobby = lobby;
            DontDestroyOnLoad(this);

            Debug.Log($"Joined Lobby With Code = {code}");
            LobbyStatic.LogPlayersInLobby(lobby);

            LobbyStatic.LoadLobbyRoom();
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
                        {
                            "PlayerName",
                            new PlayerDataObject(
                                PlayerDataObject.VisibilityOptions.Public,
                                StartupManager.PlayerName
                            )
                        }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

            currentLobby.currentLobby = lobby;
            DontDestroyOnLoad(this);

            Debug.Log($"Joined Lobby With Id = {lobbyId}");
            LobbyStatic.LogPlayersInLobby(lobby);

            lobbySetupPanel.SetActive(false);
            
            LobbyStatic.LoadLobbyRoom();
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
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = new Player(AuthenticationService.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            "PlayerName",
                            new PlayerDataObject(
                                PlayerDataObject.VisibilityOptions.Public,
                                StartupManager.PlayerName
                            )
                        }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            currentLobby.currentLobby = lobby;
            DontDestroyOnLoad(this);

            Debug.Log($"Joined Lobby With Quick Join = {lobby.Id}");
            LobbyStatic.LogPlayersInLobby(lobby);

            lobbySetupPanel.SetActive(false);
            
            LobbyStatic.LoadLobbyRoom();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}