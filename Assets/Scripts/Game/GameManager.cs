using UnityEngine;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;

public class GameManager : MonoBehaviour
{
    private string defaultPrefabId = "palyaco";
    private const string Key = "CustomizationData";

    private bool isCustomizationApplied = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await Task.Delay(500);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("✅ Oturum açıldı: " + AuthenticationService.Instance.PlayerId);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("⚠️ Oturum açılırken hata: " + e.Message);
            }
        }

        await CheckOrDefaultCharacter();

        var visualSetup = Object.FindFirstObjectByType<PlayerVisualSetup>();
        if (visualSetup != null)
            visualSetup.LoadVisual();
        else
            Debug.LogWarning("🚫 Visual setup sahnede bulunamadı.");

        await ApplyCustomizationDirectly();
    }

    private async Task ApplyCustomizationDirectly()
    {
        if (isCustomizationApplied) return;

        await Task.Delay(500); // Sahne otursun

        GameObject containerGO = GameObject.Find("VisualContainer");
        if (containerGO == null)
        {
            Debug.LogWarning("❌ VisualContainer sahnede bulunamadı.");
            return;
        }

        if (containerGO.transform.childCount == 0)
        {
            Debug.LogWarning("⚠️ VisualContainer içinde karakter prefab yok.");
            return;
        }

        GameObject characterGO = containerGO.transform.GetChild(0).gameObject;
        var builder = characterGO.GetComponent<CharacterBuilder>();

        if (builder == null)
        {
            Debug.LogWarning("❌ CharacterBuilder sahnedeki karakter prefabında yok.");
            return;
        }

        var data = GameState.LocalPlayerData;
        if (data == null)
        {
            Debug.LogWarning("❗ GameState.LocalPlayerData null.");
            return;
        }

        data.EnsureCustomizationReady();
        builder.ApplyCustomization(data);

        Debug.Log("✅ GameManager → Kayıtlı karakter görünümü sahneye uygulandı.");
        isCustomizationApplied = true;
    }

    public async Task CheckOrDefaultCharacter()
    {
        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { Key });

        if (result.TryGetValue(Key, out var item))
        {
            string json = null;

            try
            {
                json = item.Value.GetAs<string>();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("⛔ Cloud Save → item.Value string olarak alınamadı: " + e.Message);
                json = item.Value?.ToString();
            }

            if (string.IsNullOrEmpty(json) || json.StartsWith("Unity.Services") || json.Length < 10)
            {
                Debug.LogWarning("⛔ Cloud Save → veri geçersiz: " + json);
                ApplyDefaultCharacter();
                return;
            }

            Debug.Log("📥 Cloud'dan gelen JSON:\n" + json);

            try
            {
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                if (data == null)
                    throw new System.Exception("FromJson sonucu null.");

                data.EnsureCustomizationReady();
                data.RestoreCustomizationData();

                if (data.CustomizationData == null || data.CustomizationData.Count == 0)
                {
                    Debug.LogWarning("⚠️ Cloud verisinde CustomizationData boş, default atama yapılıyor.");
                    ApplyDefaultCharacter();
                }
                else
                {
                    GameState.LocalPlayerData = data;
                }

                Debug.Log("☁️ Cloud Save → karakter görünümü geri yüklendi.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Cloud Save verisi parse edilemedi: " + e.Message);
                ApplyDefaultCharacter();
            }
        }
        else
        {
            Debug.Log("📦 Cloud Save → veri bulunamadı.");
            ApplyDefaultCharacter();
        }
    }

    public void ApplyDefaultCharacter()
    {
        var defaultData = new PlayerData
        {
            PlayerName = "Player",
            PrefabId = defaultPrefabId,
            SelectedSkin = "Basic",
            CustomizationData = new Dictionary<string, string>()
            {
                { "Body", "palyaco" }
            }
        };

        defaultData.BakeCustomizationData();
        GameState.LocalPlayerData = defaultData;
        Debug.Log("🎭 Varsayılan karakter hazırlandı: palyaco.");
    }
}
