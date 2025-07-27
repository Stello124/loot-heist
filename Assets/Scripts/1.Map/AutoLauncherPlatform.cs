using UnityEngine;
using System.Collections;

public class AutoLauncherPlatform : MonoBehaviour
{
    public float launchForce = 15f;      // Oyuncuya verilecek zıplatma kuvveti
    public float moveAmount = 2f;        // Platformun yukarı çıkma mesafesi
    public float moveSpeed = 10f;        // Yukarı/aşağı hareket hızı
    public float interval = 3f;          // Kaç saniyede bir tekrar etsin

    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + Vector3.up * moveAmount;

        StartCoroutine(LauncherLoop());
    }

    IEnumerator LauncherLoop()
    {
        while (true)
        {
            // Yukarı hareket
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            // Platform üstündeki oyuncuyu fırlat
            Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2f);
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Player"))
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                        rb.AddForce(Vector3.up * launchForce, ForceMode.VelocityChange);
                    }
                }
            }

            // Kısa bekleme sonra aşağı in
            yield return new WaitForSeconds(0.1f);

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(endPos, startPos, t);
                yield return null;
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
