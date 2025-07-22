using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyStatic
{
    public static void LogPlayersInLobby(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player ID = " + player.Id);

            if (player.Data.TryGetValue("PlayerName", out var nameObj))
                Debug.Log("Player Name = " + nameObj.Value);
            else
                Debug.Log("Player Name = Ýsimsiz");
        }
    }

    public static void LogLobby(Lobby lobby)
    {
        Debug.Log("Lobby Id : " + lobby.Id + "\n" + "GameMode = " + lobby.Data["GameMode"].Value);
    }

    public static void LoadLobbyRoom()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}