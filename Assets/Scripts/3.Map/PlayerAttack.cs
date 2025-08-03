using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 4f; // Vurma mesafesi artırıldı
    public LayerMask playerLayer;
    public Transform attackOrigin;
    public AudioClip punchSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Collider kontrolü (raycast için gerekli)
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            Debug.Log($"✅ PlayerAttack başlatıldı - Collider: {playerCollider.GetType().Name}, Range: {attackRange}m (MANUEL EKLENDİ)");
        }
        else
        {
            Debug.LogWarning($"⚠️ PlayerAttack - Collider YOK! Raycast hedef olamaz: {gameObject.name}");
        }
        
        // Animator debug
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log($"✅ PlayerAttack Animator bulundu: {animator.runtimeAnimatorController?.name}");
        }
        else
        {
            Debug.LogWarning($"❌ PlayerAttack - Animator YOK! {gameObject.name}");
        }
        
        // NetworkObject debug
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            Debug.Log($"✅ PlayerAttack NetworkObject - IsOwner: {netObj.IsOwner}, ClientId: {netObj.OwnerClientId}");
        }
        else
        {
            Debug.LogWarning($"❌ PlayerAttack - NetworkObject YOK! {gameObject.name}");
        }
    }

    void Update()
    {
        // Debug: Component çalışıyor mu?
        if (Input.GetKeyDown(KeyCode.P)) // P tuşu ile test
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            bool isOwner = netObj != null && netObj.IsOwner;
            Debug.Log($"🔧 PlayerAttack UPDATE çalışıyor! IsOwner: {isOwner}, Name: {gameObject.name}");
            
            // Scale debug
            Debug.Log($"🔧 Current Scale: {transform.localScale}");
            Debug.Log($"🔧 Current Position: {transform.position}");
        }
        
        // Sadece owner için input al
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsOwner) 
        {
            if (Input.GetMouseButtonDown(0)) // Debug için
            {
                Debug.Log($"❌ Sol click alındı ama IsOwner=false: {gameObject.name}");
            }
            return;
        }
        
        // Sol click (fare) veya sol kontrol tuşu
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log($"🎮 Vurma input alındı! Mouse: {Input.GetMouseButtonDown(0)}, LCtrl: {Input.GetKeyDown(KeyCode.LeftControl)}, Player: {gameObject.name}");
            TryAttack();
        }
    }

    void TryAttack()
    {
        Debug.Log($"👊 TryAttack çağrıldı - {gameObject.name}");
        
        // Attack animasyonu oynat
        PlayAttackAnimation();
        
        // AttackOrigin kontrolü
        if (attackOrigin == null)
        {
            attackOrigin = transform; // Fallback: kendi transform'u kullan
            Debug.LogWarning($"⚠️ AttackOrigin null! Transform kullanılıyor: {transform.name}");
        }
        
        Debug.Log($"📍 Attack Origin: {attackOrigin.name}, Position: {attackOrigin.position}, Forward: {attackOrigin.forward}");
        
        // Bomba kontrolü
        GameObject currentBombHolder = BombManager.Instance.GetCurrentBombHolder();
        bool hasBomb = currentBombHolder == this.gameObject;
        Debug.Log($"💣 Bomba kontrolü: CurrentHolder={currentBombHolder?.name}, ThisPlayer={gameObject.name}, HasBomb={hasBomb}");
        
        if (!hasBomb)
        {
            Debug.Log("❌ Bu oyuncuda bomba yok, saldırı iptal");
            return;
        }
        
        Debug.Log("✅ Bombalı oyuncu vurma yapıyor!");
        
        RaycastHit hit;
        Vector3 rayStart = attackOrigin.position;
        Vector3 rayDirection = attackOrigin.forward;
        
        Debug.Log($"🔫 Raycast atılıyor: Start={rayStart}, Direction={rayDirection}, Range={attackRange}m");
        
        // Debug için raycast'i görselleştir (Scene view'da görünür, 3 saniye)
        Debug.DrawRay(rayStart, rayDirection * attackRange, Color.red, 3f);
        
        // Tüm objeleri kontrol et, layer mask kullanma
        if (Physics.Raycast(rayStart, rayDirection, out hit, attackRange))
        {
            Debug.Log($"🎯 Raycast HİT! Obje: {hit.collider.name} - Tag: {hit.collider.tag} - Distance: {hit.distance:F2}m");
            
            // Hit noktasını göster (yeşil çizgi, 3 saniye)
            Debug.DrawLine(rayStart, hit.point, Color.green, 3f);
            
            GameObject hitPlayer = hit.collider.gameObject;

            if (hitPlayer.CompareTag("Player"))
            {
                NetworkObject hitNetObj = hitPlayer.GetComponent<NetworkObject>();
                if (hitNetObj != null)
                {
                    ulong targetClientId = hitNetObj.OwnerClientId;
                    NetworkObject myNetObj = GetComponent<NetworkObject>();
                    ulong myClientId = myNetObj != null ? myNetObj.OwnerClientId : 999;
                    
                    Debug.Log($"🎯 TARGET ANALİZİ:");
                    Debug.Log($"  - Vuran: {gameObject.name} (Client {myClientId})");
                    Debug.Log($"  - Hedef: {hitPlayer.name} (Client {targetClientId})");
                    Debug.Log($"  - NetworkObject: {hitNetObj.name}, IsOwner: {hitNetObj.IsOwner}");
                    
                    Debug.Log($"✅ BOMBA TRANSFERİ! {gameObject.name} (Client {myClientId}) → {hitPlayer.name} (Client {targetClientId})");
                    
                    // Vurulma animasyonu tetikle
                    PlayerAttack hitPlayerAttack = hitPlayer.GetComponent<PlayerAttack>();
                    if (hitPlayerAttack != null)
                    {
                        hitPlayerAttack.PlayHitReaction();
                        Debug.Log($"🤕 {hitPlayer.name} vurulma animasyonu tetiklendi");
                    }
                    
                    // BombManager'a direkt transfer isteği gönder
                    if (BombManager.Instance != null)
                    {
                        BombManager.Instance.TransferBombToClient(targetClientId);
                        Debug.Log($"📡 Transfer isteği gönderildi: Target {targetClientId}");
                    }
                    
                    // Ses efekti - sadece saldıran oyuncu duyar
                    if (punchSound != null && audioSource != null)
                        audioSource.PlayOneShot(punchSound);
                }
                else
                {
                    Debug.LogError($"❌ {hitPlayer.name} NetworkObject yok!");
                }
            }
            else
            {
                Debug.Log($"❌ Hit obje Player değil: {hit.collider.tag}");
            }
        }
        else
        {
            Debug.Log($"❌ Raycast hiçbir şeye çarpmadı! Start: {rayStart}, Direction: {rayDirection}, Range: {attackRange}m");
            Debug.Log($"🔍 Nearby objects check başlatılıyor...");
            
            // Yakındaki objeleri bul
            Collider[] nearbyColliders = Physics.OverlapSphere(rayStart, attackRange);
            Debug.Log($"🔍 Yakında {nearbyColliders.Length} obje var:");
            foreach (Collider col in nearbyColliders)
            {
                if (col.gameObject != this.gameObject) // Kendisi hariç
                {
                    Debug.Log($"  - {col.gameObject.name} (Tag: {col.tag}, Distance: {Vector3.Distance(rayStart, col.transform.position):F2}m)");
                }
            }
        }
    }
    
    /// <summary>
    /// Attack animasyonu oynatır - Geçici controller değişimi ile
    /// </summary>
    void PlayAttackAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("❌ PlayerAttack: Animator component bulunamadı!");
            return;
        }
        
        Debug.Log($"🎬 Attack Controller: {animator.runtimeAnimatorController?.name}");
        
        // Direkt attack animasyonu oyna
        try 
        { 
            animator.Play("Attack_Punch"); 
            Debug.Log("👊 Attack_Punch animasyonu BAŞLADI!"); 
        }
        catch 
        { 
            try 
            { 
                animator.SetTrigger("Attack"); 
                Debug.Log("👊 Attack trigger GÖNDERİLDİ!");
            }
            catch 
            { 
                Debug.LogWarning("❌ Attack animasyonu/trigger bulunamadı!");
            }
        }
    }
    
    /// <summary>
    /// Attack için geçici controller değişimi ve animasyon oynatma
    /// </summary>
    private System.Collections.IEnumerator PlayAttackWithControllerSwitch(Animator animator)
    {
        // Orijinal controller'ı kaydet
        RuntimeAnimatorController originalController = animator.runtimeAnimatorController;
        
        // Attack controller'ını yükle
        RuntimeAnimatorController attackController = Resources.Load<RuntimeAnimatorController>("ASSETS/ithappy/Creative_Characters_FREE/Animations/AnimationController");
        
        if (attackController != null)
        {
            // Attack controller'ına geç
            animator.runtimeAnimatorController = attackController;
            Debug.Log($"🔄 Attack controller'ına geçildi: {attackController.name}");
            
            // 1 frame bekle (controller değişimi için)
            yield return null;
            
            // Attack animasyonu oyna
            bool animationPlayed = false;
            
            try 
            { 
                animator.Play("Attack_Punch"); 
                Debug.Log("👊 Attack_Punch animasyonu başlatıldı!"); 
                animationPlayed = true;
            }
            catch 
            { 
                Debug.Log("⚠️ Attack_Punch bulunamadı, trigger deneniyor");
                try 
                { 
                    animator.SetTrigger("Attack"); 
                    Debug.Log("👊 Attack trigger gönderildi!");
                    animationPlayed = true;
                }
                catch 
                { 
                    Debug.Log("❌ Attack trigger de yok"); 
                }
            }
            
            // Attack animasyonu süresini bekle
            if (animationPlayed)
            {
                yield return new WaitForSeconds(1.2f); // Attack animasyonu süresi
                Debug.Log("✅ Attack animasyonu tamamlandı");
            }
            else
            {
                yield return new WaitForSeconds(0.3f); // Kısa bekleme
            }
            
            // Orijinal controller'a geri dön
            animator.runtimeAnimatorController = originalController;
            Debug.Log($"🔄 Orijinal controller'a geri dönüldü: {originalController?.name}");
        }
        else
        {
            Debug.LogWarning("❌ AnimationController Resources'tan yüklenemedi!");
            Debug.Log("👊 Vurma hareketi yapıldı (animasyon yok)");
        }
    }
    
    /// <summary>
    /// Animator'da belirtilen state var mı kontrol eder
    /// </summary>
    bool HasAnimatorState(Animator animator, string stateName)
    {
        if (animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName || stateName.Contains(clip.name))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Animator'da belirtilen parameter var mı kontrol eder
    /// </summary>
    bool HasAnimatorParameter(Animator animator, string parameterName)
    {
        if (animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName)
            {
                return param.type == AnimatorControllerParameterType.Trigger;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Vurulma animasyonu oynatır (başka oyuncu buna vurduğunda)
    /// </summary>
    public void PlayHitReaction()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null) return;
        
        Debug.Log("🤕 Vurulma animasyonu başlatılıyor...");
        
        // Direkt hit animasyonu oyna
        try 
        { 
            animator.Play("Hit_Reaction_Light"); 
            Debug.Log("🤕 Hit_Reaction_Light animasyonu BAŞLADI!"); 
        }
        catch 
        { 
            try 
            { 
                animator.SetTrigger("Hit"); 
                Debug.Log("🤕 Hit trigger GÖNDERİLDİ!");
            }
            catch 
            { 
                Debug.LogWarning("❌ Hit animasyonu/trigger bulunamadı!");
            }
        }
    }
    
    /// <summary>
    /// Hit reaction için geçici controller değişimi ve animasyon oynatma
    /// </summary>
    private System.Collections.IEnumerator PlayHitWithControllerSwitch(Animator animator)
    {
        // Orijinal controller'ı kaydet
        RuntimeAnimatorController originalController = animator.runtimeAnimatorController;
        
        // Attack controller'ını yükle (hit animasyonları da burada)
        RuntimeAnimatorController attackController = Resources.Load<RuntimeAnimatorController>("ASSETS/ithappy/Creative_Characters_FREE/Animations/AnimationController");
        
        if (attackController != null)
        {
            // Attack controller'ına geç
            animator.runtimeAnimatorController = attackController;
            Debug.Log($"🔄 Hit için controller'a geçildi: {attackController.name}");
            
            // 1 frame bekle (controller değişimi için)
            yield return null;
            
            // Hit animasyonu oyna
            bool animationPlayed = false;
            
            // Hit state'leri dene
            string[] hitStates = { "Hit_Reaction_Light", "Hit_Reaction", "Hit", "Hurt", "Damage" };
            foreach (string hitState in hitStates)
            {
                try
                {
                    animator.Play(hitState);
                    Debug.Log($"🤕 Vurulma animasyonu başlatıldı: {hitState}");
                    animationPlayed = true;
                    break;
                }
                catch
                {
                    Debug.Log($"⚠️ {hitState} state bulunamadı");
                }
            }
            
            // Trigger'ları dene
            if (!animationPlayed)
            {
                string[] hitTriggers = { "Hit", "Hurt", "Damage", "TakeHit" };
                foreach (string triggerName in hitTriggers)
                {
                    try
                    {
                        animator.SetTrigger(triggerName);
                        Debug.Log($"🤕 Vurulma trigger gönderildi: {triggerName}");
                        animationPlayed = true;
                        break;
                    }
                    catch
                    {
                        Debug.Log($"⚠️ {triggerName} trigger bulunamadı");
                    }
                }
            }
            
            // Hit animasyonu süresini bekle
            if (animationPlayed)
            {
                yield return new WaitForSeconds(0.8f); // Hit animasyonu süresi
                Debug.Log("✅ Hit animasyonu tamamlandı");
            }
            else
            {
                yield return new WaitForSeconds(0.2f); // Kısa bekleme
                Debug.Log("🤕 Vurulma animasyonu bulunamadı");
            }
            
            // Orijinal controller'a geri dön
            animator.runtimeAnimatorController = originalController;
            Debug.Log($"🔄 Orijinal controller'a geri dönüldü: {originalController?.name}");
        }
        else
        {
            Debug.LogWarning("❌ AnimationController Resources'tan yüklenemedi!");
            Debug.Log("🤕 Vurulma hareketi yapıldı (animasyon yok)");
        }
    }
    
    // ServerRpc artık gerekmiyor - direkt BombManager.TransferBombToClient kullanıyoruz
}

