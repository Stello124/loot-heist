using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Debug.Log("Öldü!");
            gameObject.SetActive(false); // veya baþka bir ölüm animasyonu
        }
    }
}

