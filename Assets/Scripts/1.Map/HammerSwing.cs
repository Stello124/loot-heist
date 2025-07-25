using UnityEngine;

public class HammerSwing : MonoBehaviour
{
    private float maxAngle;
    private float speed;

    void Start()
    {
        // Açı ve hız başlangıçta bir kez rastgele seçilir
        maxAngle = Random.Range(75f, 90f);
        speed = Random.Range(2f, 4f);
    }

    void Update()
    {
        float angle = maxAngle * Mathf.Sin(Time.time * speed);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
