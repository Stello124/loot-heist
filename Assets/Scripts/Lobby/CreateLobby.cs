using TMPro;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.Services.Authentication;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CreateLobby : MonoBehaviour
{
    [Header("UI Inputs")]
    public TMP_InputField lobbyname;
    public TMP_InputField lobbyCode;
    public TMP_Dropdown maxplayers;
    public TMP_Dropdown gamemode;
    public Toggle islobbyprivate;

    [Header("Panel Referansları")]
    public GameObject lobbySetupPanel;       // Paneli kapatmak için
    public LobbyRoomUI lobbyRoomUI;          // Lobby bilgilerini yazdıran script

    public async void CreateLobbyMethod()
    {
        string lobbyName = lobbyname.text;
        int maxPlayers = Convert.ToInt32(maxplayers.options[maxplayers.value].text);

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = islobbyprivate.isOn,
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
            },
            Data = new Dictionary<string, DataObject>
            {
                {
                    "GameMode",
                    new DataObject(
                        DataObject.VisibilityOptions.Public,
                        gamemode.options[gamemode.value].text,
                        DataObject.IndexOptions.S1
                    )
                }
            }
        };

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            GetComponent<CurrentLobby>().currentLobby = lobby;
            DontDestroyOnLoad(this);
            Debug.Log("Lobby Oluşturuldu ✅");

            // Panel geçişi
            lobbySetupPanel.SetActive(false);

            // UI verileri yazdır
            if (lobbyRoomUI != null)
                lobbyRoomUI.UpdateLobbyUI();

            // Kod gösterimi
            if (lobbyCode != null)
                lobbyCode.text = lobby.LobbyCode;

            // Heartbeat başlat
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 9f));
            LobbyStatic.LoadLobbyRoom();
            //SceneManager.LoadScene("LobbyRoom");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby oluşturulamadı: {e.Message}");
        }
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}