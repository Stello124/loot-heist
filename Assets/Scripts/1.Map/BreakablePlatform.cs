using UnityEngine;
using System.Collections;

public class BreakablePlatform : MonoBehaviour
{
    public GameObject platformToDisable;  // Mesh'li görsel platform objesi
    public float warningDuration = 1.5f;  // Yanıp sönme süresi
    public float blinkInterval = 0.1f;    // Yanıp sönme aralığı
    public float reappearDelay = 3f;      // Kırıldıktan sonra tekrar görünme süresi

    private Renderer platformRenderer;
    private Collider platformCollider;
    private bool isBreaking = false;

    void Start()
    {
        platformRenderer = platformToDisable.GetComponent<Renderer>();
        platformCollider = platformToDisable.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isBreaking && other.CompareTag("Player"))
        {
            StartCoroutine(BreakSequence());
        }
    }

    IEnumerator BreakSequence()
    {
        isBreaking = true;

        float timer = 0f;
        while (timer < warningDuration)
        {
            platformRenderer.enabled = !platformRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        platformRenderer.enabled = false;
        platformCollider.enabled = false;

        yield return new WaitForSeconds(reappearDelay);

        platformRenderer.enabled = true;
        platformCollider.enabled = true;
        isBreaking = false;
    }
}
