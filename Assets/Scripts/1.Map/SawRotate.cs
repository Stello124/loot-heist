using UnityEngine;

public class SawRotate : MonoBehaviour
{
    public float rotationSpeed = 360f; // Derece/saniye

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
    }
}
