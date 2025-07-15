using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Ýmleci ortalar ve gizler
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Yukarý aþaðý dönmeyi sýnýrlýyoruz

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Sadece yukarý-aþaðý kamera döner
        playerBody.Rotate(Vector3.up * mouseX); // Sað-sol tüm karakter döner
    }
}
