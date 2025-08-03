using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHearts(currentHealth);
    }

    public void TakeDamage(int amount)
    {
        if (!GameStarterManager.GameStarted) return;
        currentHealth -= amount;
        Debug.Log("Can kaldý: " + currentHealth);
        UIManager.Instance.UpdateHearts(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Karakter öldü!");
        Destroy(gameObject);
    }
}

