using UnityEngine;

public class LauncherPlatform : MonoBehaviour
{
    public float launchForce = 15f;     // Karaktere uygulanacak kuvvet
    public float moveAmount = 2f;       // Platformun yukarı çıkacağı mesafe
    public float moveSpeed = 10f;       // Platformun hareket hızı
    public float resetDelay = 0.3f;     // Platformun tekrar aşağı inme süresi

    private Vector3 startPos;
    private Vector3 endPos;
    private bool isLaunching = false;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + Vector3.up * moveAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLaunching && other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Platform yukarı zıplasın ve karakteri fırlatsın
                StartCoroutine(Launch(rb));
            }
        }
    }

    System.Collections.IEnumerator Launch(Rigidbody playerRb)
    {
        isLaunching = true;

        // Platform yukarı hızlıca çık
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Karakteri yukarı fırlat
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z); // Y ekseni sıfırlanır
        playerRb.AddForce(Vector3.up * launchForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(resetDelay);

        // Platform tekrar aşağı insin
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(endPos, startPos, t);
            yield return null;
        }

        isLaunching = false;
    }
}
