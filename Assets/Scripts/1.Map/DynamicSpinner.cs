using UnityEngine;

public class DynamicSpinner : MonoBehaviour
{
    public float baseSpeed = 50f;       // Ortalama dönme hızı
    public float speedVariance = 30f;   // Ne kadar artıp azalacak
    public float changeSpeed = 0.5f;    // Hız değişiminin yavaşlığı

    void Update()
    {
        float dynamicSpeed = baseSpeed + Mathf.Sin(Time.time * changeSpeed) * speedVariance;

        // SAAT YÖNÜNÜN TERSİ için negatif değer
        transform.Rotate(0f, -dynamicSpeed * Time.deltaTime, 0f);
    }
}
