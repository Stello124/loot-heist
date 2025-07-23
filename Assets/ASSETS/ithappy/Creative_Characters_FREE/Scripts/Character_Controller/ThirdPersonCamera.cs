using UnityEngine;

namespace Controller
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;       // Takip edilecek karakter
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);  // Kamera hedef noktasý için offset

        [Header("Kamera Ayarlarý")]
        [SerializeField] private float distance = 3.5f;  // Karaktere uzaklýk
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Dönüþ Limitleri")]
        [SerializeField] private float minY = -20f;
        [SerializeField] private float maxY = 60f;

        private Vector2 angles;           // X = Yukarý-aþaðý, Y = sað-sol
        private Vector3 lookPoint;
        private Vector3 targetPos;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (target == null)
                Debug.LogError("ThirdPersonCamera: Target atanmadý!");
        }

        private void LateUpdate()
        {
            HandleInput();
            CalculateTargetPosition();
            SmoothMove();
        }

        private void HandleInput()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            angles.y += mouseX * rotationSpeed * Time.deltaTime;
            angles.x -= mouseY * rotationSpeed * Time.deltaTime;
            angles.x = Mathf.Clamp(angles.x, minY, maxY);
        }

        private void CalculateTargetPosition()
        {
            lookPoint = target.position + offset;
            Quaternion rotation = Quaternion.Euler(angles.x, angles.y, 0f);
            Vector3 direction = rotation * Vector3.back * distance;

            targetPos = lookPoint + direction;
        }

        private void SmoothMove()
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            transform.LookAt(lookPoint);
        }
    }
}
