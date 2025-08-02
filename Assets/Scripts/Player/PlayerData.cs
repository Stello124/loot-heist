using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string PlayerName;
    public string PrefabId;
    public string SelectedSkin;

    public List<CustomizationRecord> CustomizationRecords = new List<CustomizationRecord>();
    public Dictionary<string, string> CustomizationData = new Dictionary<string, string>();

    public void RestoreCustomizationData()
    {
        CustomizationData = new Dictionary<string, string>();

        if (CustomizationRecords == null || CustomizationRecords.Count == 0)
        {
            Debug.LogWarning("❌ Restore iptal edildi → CustomizationRecords boş.");
            return;
        }

        foreach (var record in CustomizationRecords)
        {
            if (!string.IsNullOrEmpty(record.Slot) && !string.IsNullOrEmpty(record.MeshName))
            {
                CustomizationData[record.Slot] = record.MeshName;
            }
        }

        Debug.Log("✅ RestoreCustomizationData başarıyla tamamlandı.");
    }

    public void BakeCustomizationData()
    {
        CustomizationRecords = new List<CustomizationRecord>();

        if (CustomizationData == null || CustomizationData.Count == 0)
        {
            Debug.LogWarning("❌ Bake iptal edildi → CustomizationData boş.");
            return;
        }

        foreach (var kvp in CustomizationData)
        {
            CustomizationRecords.Add(new CustomizationRecord
            {
                Slot = kvp.Key,
                MeshName = kvp.Value
            });
        }

        Debug.Log("✅ BakeCustomizationData başarıyla tamamlandı.");
    }

    public void EnsureCustomizationReady()
    {
        if (CustomizationData == null)
            CustomizationData = new Dictionary<string, string>();

        if (CustomizationRecords == null)
            CustomizationRecords = new List<CustomizationRecord>();
    }

    public bool IsCustomizationLoaded
    {
        get
        {
            return CustomizationData != null && CustomizationData.Count > 0;
        }
    }
}

[System.Serializable]
public class CustomizationRecord
{
    public string Slot;
    public string MeshName;
}
