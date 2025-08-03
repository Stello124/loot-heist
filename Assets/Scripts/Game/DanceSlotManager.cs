using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Threading.Tasks;

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
            
            // Dans slot'larını cloud'dan yükle
            LoadDanceSlotsFromCloud();
            
            // Dans UI Controller'ı ekle
            EnsureDanceUIController();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnEnable()
    {
        // Sahne değişince DanceUIController'ı tekrar kontrol et
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Yeni sahneye geçince DanceUIController'ı yeniden oluştur
        Invoke("EnsureDanceUIController", 0.5f); // Kısa delay ile
        
        // 3.map için PlayerAttack ekle (Client için)
        if (scene.name == "3.map")
        {
            Invoke("EnsurePlayerAttack", 1f); // Biraz daha geç, player spawn olduktan sonra
        }
    }
    
    /// <summary>
    /// DanceUIController'ın sahnede var olduğundan emin ol
    /// </summary>
    void EnsureDanceUIController()
    {
        if (FindObjectOfType<DanceUIController>() == null)
        {
            GameObject danceUIObj = new GameObject("DanceUIController");
            danceUIObj.AddComponent<DanceUIController>();
            Debug.Log("✅ DanceUIController otomatik oluşturuldu");
        }
    }
    
    /// <summary>
    /// 3.map'te Client için PlayerAttack component'ini ekle
    /// </summary>
    void EnsurePlayerAttack()
    {
        Debug.Log("🔍 EnsurePlayerAttack çağrıldı - Client için PlayerAttack arıyor...");
        
        // Tüm NetworkObject'leri bul
        var networkObjects = FindObjectsOfType<Unity.Netcode.NetworkObject>();
        Debug.Log($"🔍 {networkObjects.Length} NetworkObject bulundu");
        
        foreach (var netObj in networkObjects)
        {
            // Sadece kendi oyuncumuz (IsOwner = true)
            if (netObj.IsOwner && (netObj.name.Contains("palyaco") || netObj.name.Contains("Player")))
            {
                Debug.Log($"🎯 Kendi player'ımız bulundu: {netObj.name}");
                
                // PlayerAttack var mı kontrol et
                PlayerAttack existingAttack = netObj.GetComponent<PlayerAttack>();
                if (existingAttack == null)
                {
                    // Yoksa ekle
                    PlayerAttack attackComp = netObj.gameObject.AddComponent<PlayerAttack>();
                    attackComp.attackRange = 4f;
                    
                    Debug.Log($"✅👊 PlayerAttack EKLENDİ (CLIENT): {netObj.name} - Range: 4m");
                    
                    // Collider kontrolü
                    Collider playerCollider = netObj.GetComponent<Collider>();
                    if (playerCollider == null)
                    {
                        // Collider yoksa ekle
                        CapsuleCollider capsule = netObj.gameObject.AddComponent<CapsuleCollider>();
                        capsule.height = 2f;
                        capsule.radius = 0.5f;
                        capsule.center = new UnityEngine.Vector3(0, 1f, 0);
                        Debug.Log($"✅📦 Collider EKLENDİ: {netObj.name}");
                    }
                    else
                    {
                        Debug.Log($"✅ Collider zaten var: {netObj.name}");
                    }
                }
                else
                {
                    Debug.Log($"✅ PlayerAttack zaten var: {netObj.name}");
                }
                
                // Sadece bir kere ekle, break
                break;
            }
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
            
            // Cloud'a kaydet
            SaveDanceSlotsToCloud();
        }
    }

    /// <summary>
    /// Animator üzerinden dansı oynatır ve event gönderir
    /// </summary>
    public void PlayDance(string danceName)
    {
        Debug.Log($"🔥 PlayDance çağrıldı: {danceName}");
        
        // Eğer characterAnimator boşsa, player'ı otomatik bul
        if (characterAnimator == null)
        {
            Debug.Log("🔥 characterAnimator null, FindPlayerAnimator çağrılıyor");
            FindPlayerAnimator();
        }
        
        if (characterAnimator == null)
        {
            Debug.LogWarning("⚠️ Animator referansı bulunamadı!");
            return;
        }

        string triggerName = $"Play{danceName}";
        string stateName = $"{danceName}_Dance";
        Debug.Log($"🔥 Trigger gönderiliyor: {triggerName}");
        Debug.Log($"🔥 State ismi: {stateName}");
        
        // ANIMATOR CONTROLLER BİLGİLERİ
        Debug.Log($"🔥 Animator Controller: {characterAnimator.runtimeAnimatorController?.name}");
        
        // YÖNTEM 1: Trigger dene
        characterAnimator.SetTrigger(triggerName);
        Debug.Log($"🔥 Trigger gönderildi: {triggerName}");
        
        // YÖNTEM 2: Tüm olası isimleri dene
        string[] possibleNames = { 
            stateName,                    // "Flair_Dance"
            danceName,                    // "Flair"
            $"{danceName}Dance",          // "FlairDance"  
            $"Base Layer.{stateName}",    // "Base Layer.Flair_Dance"
            $"Base Layer.{danceName}",    // "Base Layer.Flair"
            $"Dance_{danceName}",         // "Dance_Flair"
            $"{danceName}_Anim",          // "Flair_Anim"
            $"{danceName} Dance"          // "Flair Dance" (boşluklu)
        };
        
        bool stateFound = false;
        foreach (string possibleName in possibleNames)
        {
            try
            {
                characterAnimator.Play(possibleName);
                Debug.Log($"✅ STATE BULUNDU VE OYNADI: '{possibleName}'");
                stateFound = true;
                break;
            }
            catch (System.Exception e)
            {
                Debug.Log($"❌ Denendi ama olmadı: '{possibleName}' - {e.Message}");
            }
        }
        
        if (!stateFound)
        {
            Debug.LogWarning("🚨 HİÇBİR STATE İSMİ ÇALIŞMADI! Animator Controller'ı kontrol et!");
        }
        
        Debug.Log($"[DanceSlotManager] Playing dance: {danceName}");
        
        // 1 saniye sonra Animator state'ini kontrol et
        StartCoroutine(CheckAnimatorState(triggerName));

        // Unity Analytics event gönderimi
        AnalyticsReporter.Instance?.ReportEvent("dance_played", new Dictionary<string, object>
        {
            { "danceName", danceName },
            { "timestamp", System.DateTime.UtcNow.ToString("o") }
        });
    }
    
    /// <summary>
    /// Player'ın Animator'ını otomatik bul
    /// </summary>
    private void FindPlayerAnimator()
    {
        Debug.Log("🔥 FindPlayerAnimator çağrıldı!");
        
        // NetworkObject ile player bul
        var networkObjects = FindObjectsOfType<Unity.Netcode.NetworkObject>();
        Debug.Log($"🔥 {networkObjects.Length} NetworkObject bulundu");
        
        foreach (var netObj in networkObjects)
        {
            Debug.Log($"🔥 NetworkObject: {netObj.name}, IsOwner: {netObj.IsOwner}");
            
            if ((netObj.IsOwner && netObj.name.Contains("Player")) || 
                netObj.name.Contains("player") || 
                netObj.name.Contains("Palyaco") ||
                netObj.name.Contains("palyaco"))
            {
                Animator animator = netObj.GetComponent<Animator>();
                Debug.Log($"🔥 Hedef obje bulundu: {netObj.name}, Animator var mı: {animator != null}");
                
                if (animator != null)
                {
                    characterAnimator = animator;
                    Debug.Log($"✅ Player Animator bulundu: {netObj.name}");
                    
                    // Animator Controller kontrolü
                    if (animator.runtimeAnimatorController != null)
                    {
                        Debug.Log($"✅ Animator Controller var: {animator.runtimeAnimatorController.name}");
                    }
                    else
                    {
                        Debug.LogWarning("❌ Animator Controller yok!");
                    }
                    return;
                }
            }
        }
        
        // Eğer NetworkObject bulamazsa, normal GameObject ara
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"🔥 {allObjects.Length} GameObject kontrol ediliyor");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player") || obj.name.Contains("player") || 
                obj.name.Contains("Palyaco") || obj.name.Contains("palyaco"))
            {
                Animator animator = obj.GetComponent<Animator>();
                Debug.Log($"🔥 GameObject: {obj.name}, Animator var mı: {animator != null}");
                
                if (animator != null)
                {
                    characterAnimator = animator;
                    Debug.Log($"✅ Player Animator bulundu (normal): {obj.name}");
                    
                    // Animator Controller kontrolü
                    if (animator.runtimeAnimatorController != null)
                    {
                        Debug.Log($"✅ Animator Controller var: {animator.runtimeAnimatorController.name}");
                    }
                    else
                    {
                        Debug.LogWarning("❌ Animator Controller yok!");
                    }
                    return;
                }
            }
        }
        
        Debug.LogWarning("❌ Player Animator bulunamadı!");
    }
    
    /// <summary>
    /// Animator state'ini kontrol et (debugging için)
    /// </summary>
    IEnumerator CheckAnimatorState(string triggerName)
    {
        yield return new WaitForSeconds(1f);
        
        if (characterAnimator != null)
        {
            // Mevcut state'i kontrol et
            AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"🔥 Mevcut Animator State: {stateInfo.fullPathHash}");
            Debug.Log($"🔥 State ismi hash: {stateInfo.shortNameHash}");
            Debug.Log($"🔥 State normalizedTime: {stateInfo.normalizedTime}");
            Debug.Log($"🔥 State length: {stateInfo.length}");
            
            // Trigger state'ini kontrol et  
            foreach (AnimatorControllerParameter param in characterAnimator.parameters)
            {
                if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    Debug.Log($"🔥 Trigger {triggerName} type: {param.type}");
                    break;
                }
            }
            
            // Is Playing dans state kontrol et
            bool isPlayingDance = characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Flair_Dance") ||
                                 characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Rumba_Dance") ||
                                 characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Twerk_Dance") ||
                                 characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Twist_Dance");
            
            Debug.Log($"🔥 Dans state'i oynuyor mu: {isPlayingDance}");
        }
    }
    
    /// <summary>
    /// Dans slot'larını Unity Cloud Save'den yükler
    /// </summary>
    public async void LoadDanceSlotsFromCloud()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "DanceSlots" });

            if (savedData.TryGetValue("DanceSlots", out Item danceSlotsItem))
            {
                string[] cloudDanceSlots = danceSlotsItem.Value.GetAs<string[]>();
                
                if (cloudDanceSlots != null && cloudDanceSlots.Length == 4)
                {
                    danceSlots = cloudDanceSlots;
                    Debug.Log("✅ Dans slot'ları cloud'dan yüklendi:");
                    for (int i = 0; i < danceSlots.Length; i++)
                    {
                        Debug.Log($"  Slot {i}: {danceSlots[i] ?? "Boş"}");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Cloud'daki dans slot'ları geçersiz, varsayılan kullanılıyor");
                    InitializeDefaultSlots();
                }
            }
            else
            {
                Debug.Log("📝 Cloud'da dans slot'ları bulunamadı, varsayılan oluşturuluyor");
                InitializeDefaultSlots();
                SaveDanceSlotsToCloud();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"❌ Dans slot'ları yüklenemedi: {ex.Message}");
            InitializeDefaultSlots();
        }
    }
    
    /// <summary>
    /// Dans slot'larını Unity Cloud Save'e kaydeder
    /// </summary>
    public async void SaveDanceSlotsToCloud()
    {
        try
        {
            var data = new Dictionary<string, object> { { "DanceSlots", danceSlots } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("✅ Dans slot'ları cloud'a kaydedildi");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"❌ Dans slot'ları kaydedilemedi: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Varsayılan dans slot'larını başlatır
    /// </summary>
    private void InitializeDefaultSlots()
    {
        danceSlots = new string[4];
        // Varsayılan danslar
        danceSlots[0] = "Flair";
        danceSlots[1] = "Rumba";
        danceSlots[2] = "Twerk";
        danceSlots[3] = "Twist";
        Debug.Log("📝 Varsayılan dans slot'ları oluşturuldu: Flair, Rumba, Twerk, Twist");
    }
    
    /// <summary>
    /// Belirli bir slot'u temizler
    /// </summary>
    public void ClearDanceSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < danceSlots.Length)
        {
            string previousDance = danceSlots[slotIndex];
            danceSlots[slotIndex] = null;
            Debug.Log($"[Dance Cleared] Slot {slotIndex}: {previousDance} → Boş");
            
            // Cloud'a kaydet
            SaveDanceSlotsToCloud();
        }
    }
    
    /// <summary>
    /// Belirli bir slot'un atanmış dansını döndürür
    /// </summary>
    public string GetAssignedDance(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < danceSlots.Length)
        {
            return danceSlots[slotIndex];
        }
        return null;
    }
}