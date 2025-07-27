using UnityEngine;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;

public class CustomizationSaveManager : MonoBehaviour
{
    public static CustomizationSaveManager Instance;

    private const string Key = "CustomizationData";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // 🔹 1. Cloud Save’e yazma
    public async void SaveCustomizationToCloud(PlayerData data)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("⛔ CloudSave yazılamadı → oyuncu oturum açmamış.");
            return;
        }

        if (data == null || data.CustomizationData == null || data.CustomizationData.Count == 0)
        {
            Debug.LogWarning("⛔ Cloud Save iptal → veri boş veya eksik.");
            return;
        }

        data.BakeCustomizationData(); // ✅ Dictionary → List dönüşümü

        string json = JsonUtility.ToJson(data);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("⛔ JSON üretilemedi → Cloud Save iptal.");
            return;
        }

        Debug.Log("🧠 Cloud'a gönderilecek JSON: " + json);

        var payload = new Dictionary<string, object>
        {
            { Key, json }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
        Debug.Log("✅ Cloud Save → karakter görünümü yazıldı.");
    }

    // 🔹 2. Cloud Save’den okuma
    public async Task<PlayerData> LoadCustomizationFromCloud()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("⛔ CloudSave okunamadı → oyuncu oturum açmamış.");
            return null;
        }

        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { Key });

        if (result.TryGetValue(Key, out var item))
        {
            string json = null;

            try
            {
                // 🔥 KRİTİK: Burada Value objesinden stringi güvenle çekiyoruz
                json = item.Value.GetAs<string>();
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ item.Value okunamadı: " + e.Message);
                return CreateDefaultPlayerData();
            }

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("⛔ Cloud Save → JSON boş veya geçersiz.");
                return CreateDefaultPlayerData();
            }

            Debug.Log("📥 Cloud'dan gelen JSON: " + json);

            try
            {
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);

                if (data == null)
                    throw new System.Exception("FromJson sonucu null.");

                data.RestoreCustomizationData(); // ✅ List → Dictionary dönüşümü

                Debug.Log("☁️ Cloud Save → karakter görünümü geri yüklendi.");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Cloud Save verisi parse edilemedi: " + e.Message + "\nJSON: " + json);
                return CreateDefaultPlayerData();
            }
        }
        else
        {
            Debug.Log("📦 Cloud Save → görünüm verisi bulunamadı.");
            return CreateDefaultPlayerData();
        }
    }

    // 🔹 3. Yedek (default) veri üretici
    private PlayerData CreateDefaultPlayerData()
    {
        PlayerData defaultData = new PlayerData();

        defaultData.CustomizationRecords = new List<CustomizationRecord>()
        {
            new CustomizationRecord { Slot = "Body", MeshName = "palyaco" }
        };

        defaultData.RestoreCustomizationData();

        Debug.Log("⚙️ Default PlayerData oluşturuldu ve hazırlandı.");
        return defaultData;
    }
}
