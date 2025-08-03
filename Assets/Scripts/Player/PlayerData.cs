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
    
    // Dans/Emote Sistemi
    public string[] DanceSlots = new string[4]; // 4 dans slotu
    public List<DanceSlotRecord> DanceSlotRecords = new List<DanceSlotRecord>();

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
    
    // Dans Sistemi Metotları
    public void RestoreDanceSlots()
    {
        if (DanceSlots == null)
            DanceSlots = new string[4];

        if (DanceSlotRecords == null || DanceSlotRecords.Count == 0)
        {
            Debug.LogWarning("❌ RestoreDanceSlots iptal edildi → DanceSlotRecords boş.");
            return;
        }

        for (int i = 0; i < DanceSlotRecords.Count && i < DanceSlots.Length; i++)
        {
            DanceSlots[i] = DanceSlotRecords[i].DanceName;
        }

        Debug.Log("✅ RestoreDanceSlots başarıyla tamamlandı.");
    }

    public void BakeDanceSlots()
    {
        DanceSlotRecords = new List<DanceSlotRecord>();

        if (DanceSlots == null)
        {
            Debug.LogWarning("❌ BakeDanceSlots iptal edildi → DanceSlots boş.");
            return;
        }

        for (int i = 0; i < DanceSlots.Length; i++)
        {
            if (!string.IsNullOrEmpty(DanceSlots[i]))
            {
                DanceSlotRecords.Add(new DanceSlotRecord
                {
                    SlotIndex = i,
                    DanceName = DanceSlots[i]
                });
            }
        }

        Debug.Log("✅ BakeDanceSlots başarıyla tamamlandı.");
    }

    public void EnsureDanceSlotsReady()
    {
        if (DanceSlots == null)
            DanceSlots = new string[4];

        if (DanceSlotRecords == null)
            DanceSlotRecords = new List<DanceSlotRecord>();
    }

    public bool IsDanceSlotsLoaded
    {
        get
        {
            return DanceSlots != null && DanceSlots.Length > 0;
        }
    }
}

[System.Serializable]
public class CustomizationRecord
{
    public string Slot;
    public string MeshName;
}

[System.Serializable]
public class DanceSlotRecord
{
    public int SlotIndex;
    public string DanceName;
}
