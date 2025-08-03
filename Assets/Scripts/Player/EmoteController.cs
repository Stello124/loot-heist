using UnityEngine;
using Unity.Netcode;

/// <summary>
/// T tuşu ile emote UI açma ve dans kontrolü sistemi
/// </summary>
public class EmoteController : NetworkBehaviour
{
    [Header("Emote Settings")]
    public Animator playerAnimator;
    
    [Header("Movement Detection")]
    public float movementThreshold = 0.1f;
    
    private bool isDancing = false;
    private string currentDanceName = "";
    private Vector3 lastPosition;
    private float lastMovementTime;
    
    void Start()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
            
        lastPosition = transform.position;
            
        Debug.Log($"✅ EmoteController başlatıldı: {gameObject.name}");
    }

    void Update()
    {
        // Sadece owner için input al
        if (!IsOwner) return;
        
        HandleTKeyInput();
        HandleMovementDetection();
    }
    
    /// <summary>
    /// T tuşu kontrolü
    /// </summary>
    void HandleTKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isDancing)
            {
                // Dans sırasında T'ye basılırsa dans bitir
                StopDancing();
            }
            // UI kontrolü DanceUIController'da zaten var, burada gerek yok
        }
    }
    
    /// <summary>
    /// Hareket algılama (WASD veya pozisyon değişimi)
    /// </summary>
    void HandleMovementDetection()
    {
        if (!isDancing) return;
        
        // Klavye input kontrolü
        bool keyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                         Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                         Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                         Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);
        
        // Pozisyon değişimi kontrolü
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        bool positionChanged = distanceMoved > movementThreshold;
        
        if (keyPressed || positionChanged)
        {
            Debug.Log($"🚶 Hareket algılandı! Key: {keyPressed}, Position: {positionChanged} ({distanceMoved:F3}m)");
            StopDancing();
        }
        
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// UI kontrolü DanceUIController'da yapılıyor
    /// </summary>
    
    /// <summary>
    /// UI'dan dans seçildiğinde çağrılır
    /// </summary>
    public void OnDanceSelected(string danceName)
    {
        Debug.Log($"💃 Dans seçildi: {danceName}");
        
        // UI artık DanceUIController'da yönetiliyor
        
        // Dansı başlat
        StartDancing(danceName);
        
        // Multiplayer: Diğer oyunculara bildir
        if (IsOwner)
            PlayDanceServerRpc(danceName);
    }
    
    /// <summary>
    /// Dansı başlatır
    /// </summary>
    public void StartDancing(string danceName)
    {
        if (playerAnimator == null) return;
        
        isDancing = true;
        currentDanceName = danceName;
        lastPosition = transform.position;
        
        // Animator trigger
        playerAnimator.SetTrigger($"Play{danceName}");
        
        Debug.Log($"💃 Dans başladı: {danceName}");
    }
    
    /// <summary>
    /// Dansı durdurur
    /// </summary>
    public void StopDancing()
    {
        if (!isDancing) return;
        
        isDancing = false;
        
        if (playerAnimator != null)
        {
            // Dans animasyonunu durdur - idle state'e geçir
            // "StopDance" trigger varsa kullan, yoksa "Idle" kullan
            if (HasTrigger(playerAnimator, "StopDance"))
            {
                playerAnimator.SetTrigger("StopDance");
            }
            else if (HasTrigger(playerAnimator, "Idle"))
            {
                playerAnimator.SetTrigger("Idle");
            }
            else
            {
                // Fallback: Bool parameter kullan
                playerAnimator.SetBool("IsIdle", true);
                playerAnimator.SetBool("IsDancing", false);
            }
        }
        
        Debug.Log($"🛑 Dans durduruldu: {currentDanceName}");
        currentDanceName = "";
    }
    
    /// <summary>
    /// Multiplayer: Dansı tüm oyunculara bildir
    /// </summary>
    [ServerRpc]
    void PlayDanceServerRpc(string danceName)
    {
        PlayDanceClientRpc(danceName);
    }
    
    [ClientRpc]
    void PlayDanceClientRpc(string danceName)
    {
        // Owner dışındaki oyuncularda da dansı başlat
        if (!IsOwner)
        {
            StartDancing(danceName);
        }
    }
    
    /// <summary>
    /// Slot'tan dans ismi al
    /// </summary>
    public void PlayDanceFromSlot(int slotIndex)
    {
        if (DanceSlotManager.Instance != null && 
            slotIndex >= 0 && slotIndex < DanceSlotManager.Instance.danceSlots.Length)
        {
            string danceName = DanceSlotManager.Instance.danceSlots[slotIndex];
            
            if (!string.IsNullOrEmpty(danceName))
            {
                OnDanceSelected(danceName);
            }
            else
            {
                Debug.LogWarning($"⚠️ Slot {slotIndex} boş!");
            }
        }
    }
    
    /// <summary>
    /// Dans durumu kontrolü (UI için)
    /// </summary>
    public bool IsDancing => isDancing;
    
    /// <summary>
    /// Animator'da trigger parametresi var mı kontrol et
    /// </summary>
    bool HasTrigger(Animator animator, string triggerName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }
}