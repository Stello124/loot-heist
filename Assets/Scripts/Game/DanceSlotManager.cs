using UnityEngine;
using System.Collections.Generic;

public class DanceSlotManager : MonoBehaviour
{
    public static DanceSlotManager Instance;
    public string[] danceSlots = new string[4];
    public Animator characterAnimator;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // sahne geçse bile danslar korunur
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Dansı ilgili slota atar ve event gönderir
    /// </summary>
    public void AssignDance(int slotIndex, string danceName)
    {
        if (slotIndex >= 0 && slotIndex < danceSlots.Length)
        {
            danceSlots[slotIndex] = danceName;
            Debug.Log($"[Dance Assigned] Slot {slotIndex}: {danceName}");

            // Unity Analytics event gönderimi
            AnalyticsReporter.Instance?.ReportEvent("dance_selected", new Dictionary<string, object>
            {
                { "slotIndex", slotIndex },
                { "danceName", danceName },
                { "timestamp", System.DateTime.UtcNow.ToString("o") }
            });
        }
    }

    /// <summary>
    /// Animator üzerinden dansı oynatır ve event gönderir
    /// </summary>
    public void PlayDance(string danceName)
    {
        if (characterAnimator == null)
        {
            Debug.LogWarning("⚠️ Animator referansı eksik!");
            return;
        }

        characterAnimator.SetTrigger($"Play{danceName}");
        Debug.Log($"[DanceSlotManager] Playing dance: {danceName}");

        // Unity Analytics event gönderimi
        AnalyticsReporter.Instance?.ReportEvent("dance_played", new Dictionary<string, object>
        {
            { "danceName", danceName },
            { "timestamp", System.DateTime.UtcNow.ToString("o") }
        });
    }
}