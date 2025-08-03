using UnityEngine;
using Unity.Netcode;

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
    }

    void Update()
    {
        // Debug: Component çalışıyor mu?
        if (Input.GetKeyDown(KeyCode.P)) // P tuşu ile test
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            bool isOwner = netObj != null && netObj.IsOwner;
            Debug.Log($"🔧 PlayerAttack UPDATE çalışıyor! IsOwner: {isOwner}, Name: {gameObject.name}");
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
    
    // ServerRpc artık gerekmiyor - direkt BombManager.TransferBombToClient kullanıyoruz
}

