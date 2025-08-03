using UnityEngine;
using Controller;

/// <summary>
/// ThirdPersonCamera'nın oyun başında 0,0,0'a gitmesini önler
/// Player spawn olana kadar kamerayı sabit tutar
/// </summary>
public class CameraStartFix : MonoBehaviour
{
    private Camera cam;
    private ThirdPersonCamera thirdPersonCam;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool playerAssigned = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        thirdPersonCam = GetComponent<ThirdPersonCamera>();
        
        // Başlangıç pozisyonunu kaydet
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // ThirdPersonCamera'yı geçici olarak devre dışı bırak
        if (thirdPersonCam != null)
        {
            thirdPersonCam.enabled = false;
            Debug.Log("🎥 CameraStartFix: ThirdPersonCamera geçici olarak devre dışı");
        }
        
        // Player atanana kadar sabit pozisyonda tut
        InvokeRepeating(nameof(CheckPlayerAssignment), 0.1f, 0.1f);
    }

    void CheckPlayerAssignment()
    {
        // ThirdPersonCamera'da player var mı kontrol et
        if (thirdPersonCam != null && !playerAssigned)
        {
            // Reflection ile player field'ını kontrol et
            var playerField = typeof(PlayerCamera).GetField("m_Player", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (playerField != null)
            {
                Transform player = (Transform)playerField.GetValue(thirdPersonCam);
                if (player != null)
                {
                    // Player atandı - ThirdPersonCamera'yı aktif et
                    thirdPersonCam.enabled = true;
                    playerAssigned = true;
                    CancelInvoke(nameof(CheckPlayerAssignment));
                    
                    Debug.Log("✅ CameraStartFix: Player atandı, ThirdPersonCamera aktif edildi");
                    
                    // Bu script'i sil - artık gerek yok
                    Destroy(this);
                }
            }
        }
    }

    void LateUpdate()
    {
        // Player atanmamışsa kamerayı başlangıç pozisyonunda tut
        if (!playerAssigned)
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }
}