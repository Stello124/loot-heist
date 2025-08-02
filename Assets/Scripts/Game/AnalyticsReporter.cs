using Unity.Services.Analytics;
using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Core;

public class AnalyticsReporter : MonoBehaviour
{
    public static AnalyticsReporter Instance;

    [SerializeField] private bool debugMode = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    async void Start()
    {
        await InitializeAnalytics();
    }

    private async System.Threading.Tasks.Task InitializeAnalytics()
    {
        try
        {
            // Unity Services initialize edilmiş mi kontrol et
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogWarning("⚠️ Unity Services henüz initialize edilmedi, bekleniyor...");
                await UnityServices.InitializeAsync();
            }

            // Analytics service durumunu kontrol et
            if (AnalyticsService.Instance != null)
            {
                Debug.Log("✅ Analytics Service hazır");
                Debug.Log($"📊 Analytics State: {UnityServices.State}");

                // Test eventi gönder
                await System.Threading.Tasks.Task.Delay(1000);
                TestAnalyticsConnection();
            }
            else
            {
                Debug.LogError("❌ Analytics Service null!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Analytics initialize hatası: {ex.Message}");
        }
    }

    private void TestAnalyticsConnection()
    {
        ReportEvent("analytics_test_connection", new Dictionary<string, object>
        {
            { "timestamp", System.DateTime.Now.ToString() },
            { "test_parameter", "connection_test" }
        });

        Debug.Log("🧪 Test eventi gönderildi: analytics_test_connection");
    }

    /// <summary>
    /// Sadece event adıyla gönderim
    /// </summary>
    public void ReportEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogWarning("⚠️ Event adı boş olamaz");
            return;
        }

        if (!IsAnalyticsReady())
            return;

        try
        {
            if (debugMode)
                Debug.Log($"📡 Event gönderiliyor: {eventName}");

            CustomEvent customEvent = new CustomEvent(eventName);
            AnalyticsService.Instance.RecordEvent(customEvent);

            if (debugMode)
                Debug.Log($"✅ Event gönderildi: {eventName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event gönderme hatası: {ex.Message}");
        }
    }

    /// <summary>
    /// Event adı + payload ile gönderim
    /// </summary>
    public void ReportEvent(string eventName, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogWarning("⚠️ Event adı boş olamaz");
            return;
        }

        if (!IsAnalyticsReady())
            return;

        try
        {
            if (debugMode)
                Debug.Log($"📡 Event gönderiliyor: {eventName}, Data: {data?.Count ?? 0} alan");

            CustomEvent customEvent = new CustomEvent(eventName);

            if (data != null)
            {
                foreach (var pair in data)
                {
                    if (pair.Value != null)
                    {
                        customEvent[pair.Key] = pair.Value;

                        if (debugMode)
                            Debug.Log($"   - {pair.Key}: {pair.Value}");
                    }
                }
            }

            AnalyticsService.Instance.RecordEvent(customEvent);

            if (debugMode)
                Debug.Log($"✅ Event gönderildi: {eventName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event gönderme hatası: {ex.Message}");
        }
    }

    private bool IsAnalyticsReady()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("⚠️ Unity Services initialize edilmedi");
            return false;
        }

        if (AnalyticsService.Instance == null)
        {
            Debug.LogWarning("⚠️ Analytics Service null");
            return false;
        }

        return true;
    }

    // Debug için manuel test metodu
    [ContextMenu("Test Analytics Event")]
    public void ManualTestEvent()
    {
        ReportEvent("manual_test", new Dictionary<string, object>
        {
            { "test_time", System.DateTime.Now.ToString() },
            { "platform", Application.platform.ToString() }
        });
    }
}