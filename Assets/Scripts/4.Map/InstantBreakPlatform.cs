using UnityEngine;

public class OneTimeBreakPlatform : MonoBehaviour
{
    public GameObject platformToDestroy; // Kırılacak platform (görsel + collider içeren)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(platformToDestroy);
        }
    }
}
