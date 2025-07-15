using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;    // Yürüyüş hızı
    public float jumpForce = 7f;     // Zıplama kuvveti

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Rigidbody ayarları önerisi:
        // rb.mass = 1f;
        // rb.drag = 0f;
        // rb.angularDrag = 0.05f;
        // Rigidbody Constraints ayarlarını kontrol et, Freeze Position X,Y,Z kapalı olsun
    }

    void Update()
    {
        // Zıplama kontrolü Update'de yapılır
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        // Hareket inputu FixedUpdate'de alınır
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Transform cam = Camera.main.transform;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move = right * x + forward * z;
        move *= moveSpeed;

        // Yatayda hız veriyoruz, düşey hızı koruyoruz
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Yere temas edildiğinde zıplama hakkı yenilenir
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}

