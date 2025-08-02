using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;

public class UnityInitializer : MonoBehaviour
{
    public static UnityInitializer Instance;

    async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeUnityServices();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("✅ Unity Services başarıyla başlatıldı");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Unity Services başlatılamadı: {ex.Message}");
        }
    }
}