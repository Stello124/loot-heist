using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyData : MonoBehaviour
{
    public static LobbyData Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Relay kodunu Lobby Data'ya ekle (Host kullanacak)
    public async Task SetRelayCodeToLobby(string lobbyId, string relayCode)
    {
        if (string.IsNullOrEmpty(lobbyId) || string.IsNullOrEmpty(relayCode))
        {
            Debug.LogError("❌ Lobby ID veya Relay Code boş!");
            return;
        }

        try
        {
            var data = new Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
            };

            var options = new UpdateLobbyOptions { Data = data };
            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);

            Debug.Log($"✅ Relay kodu Lobby'ya eklendi: {relayCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"❌ Relay kodu eklenirken hata: {e.Message}");
        }
    }

    // Lobby'dan relay kodunu al (Client kullanacak)
    public async Task<string> GetRelayCodeFromLobby(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("❌ Lobby ID boş!");
            return null;
        }

        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);

            if (lobby.Data != null && lobby.Data.ContainsKey("RelayCode"))
            {
                string relayCode = lobby.Data["RelayCode"].Value;
                Debug.Log($"✅ Lobby'dan relay kodu alındı: {relayCode}");
                return relayCode;
            }
            else
            {
                Debug.Log("⚠️ Lobby'da relay kodu henüz yok");
                return null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"❌ Relay kodu alınırken hata: {e.Message}");
            return null;
        }
    }

    // Lobby'da relay kodu var mı kontrol et
    public async Task<bool> HasRelayCode(string lobbyId)
    {
        string code = await GetRelayCodeFromLobby(lobbyId);
        return !string.IsNullOrEmpty(code);
    }
}