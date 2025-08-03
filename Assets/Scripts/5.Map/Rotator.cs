using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float maxSpeed = 100f;
    public float acceleration = 5f;

    void Update()
    {
        if (rotationSpeed < maxSpeed)
            rotationSpeed += acceleration * Time.deltaTime;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!GameStarterManager.GameStarted) return;
        HealthController health = other.GetComponent<HealthController>();
        if (health != null)
        {
            health.TakeDamage(1);
        }
    }

}

