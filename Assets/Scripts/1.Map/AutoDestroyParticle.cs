using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(gameObject, 2f); // Garanti için yedek zaman
        }
    }
}
