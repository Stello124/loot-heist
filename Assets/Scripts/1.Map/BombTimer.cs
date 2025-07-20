using UnityEngine;

public class BombTimer : MonoBehaviour
{
    public float fuseTime = 5f; // Toplam süre
    public GameObject explosionEffect; // Patlama efekti
    public MeshRenderer bombRenderer; // Bombanın MeshRenderer bileşeni
    public Color flashColor = Color.red;
    private Color originalColor;

    private float timer;
    private Material runtimeMat;

    void Start()
    {
        timer = fuseTime;

        if (bombRenderer == null)
        {
            Debug.LogError("Bomb Renderer atanmadı!");
            return;
        }

        // Runtime material ile çalış
        runtimeMat = bombRenderer.material;
        originalColor = runtimeMat.color;

        StartCoroutine(FlashAndExplode());
    }

    System.Collections.IEnumerator FlashAndExplode()
    {
        while (timer > 0f)
        {
            float intensity = Mathf.InverseLerp(fuseTime, 0f, timer); // 0-1 arası değer
            float flashInterval = Mathf.Lerp(0.5f, 0.05f, intensity); // Zaman azalınca interval düşer

            runtimeMat.color = flashColor;
            yield return new WaitForSeconds(flashInterval / 2f);

            runtimeMat.color = originalColor;
            yield return new WaitForSeconds(flashInterval / 2f);

            timer -= flashInterval;
        }

        // Patlama efekti oluştur
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Debug.Log("💥 Boom!");
        Destroy(gameObject);
    }
}
