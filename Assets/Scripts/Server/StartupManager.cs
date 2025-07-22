using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class StartupManager : MonoBehaviour
{
    public static string PlayerName { get; private set; }

    async void Awake()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        string playerId = AuthenticationService.Instance.PlayerId;
        Debug.Log("PlayerID: " + playerId);

        await LoadOrCreatePlayerName();
    }

    async Task LoadOrCreatePlayerName()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "PlayerName" });

            if (savedData.TryGetValue("PlayerName", out Item playerNameItem))
            {
                PlayerName = playerNameItem.Value.GetAsString();
                Debug.Log("PlayerName Yüklendi: " + PlayerName);
            }
            else
            {
                PlayerName = "Player_" + Random.Range(1000, 9999);
                var newData = new Dictionary<string, object> { { "PlayerName", PlayerName } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(newData);
                Debug.Log("Yeni PlayerName Atandý: " + PlayerName);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("PlayerName yüklenemedi: " + ex.Message);
        }
    }

    public static async void SetNewPlayerName(string newName)
    {
        PlayerName = newName;
        var data = new Dictionary<string, object> { { "PlayerName", newName } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("Yeni PlayerName Kaydedildi: " + newName);
    }
}