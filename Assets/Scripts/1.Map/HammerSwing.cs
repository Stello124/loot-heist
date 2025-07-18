using UnityEngine;

public class HammerSwing : MonoBehaviour
{
    public float maxAngle = 45f;  // Salınım açısı
    public float speed = 2f;      // Salınım hızı

    private void Update()
    {
        float angle = maxAngle * Mathf.Sin(Time.time * speed);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
