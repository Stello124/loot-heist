using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float boostedSpeed = 10f;
    private float currentSpeed;

    public GameObject speedEffectPrefab;
    public GameObject jumpEffectPrefab;

    public float jumpForce = 5f;
    public float boostedJumpForce = 10f; // JumpChest için
    private float currentJumpForce;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    private float cameraPitch = 0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = moveSpeed;
        currentJumpForce = jumpForce;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Kamera kontrolü
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        // Zýplama
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 velocity = move * currentSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpeedChest"))
        {
            StartCoroutine(HandleSpeedChest(other.gameObject));
        }
        else if (other.CompareTag("JumpChest"))
        {
            StartCoroutine(HandleJumpChest(other.gameObject));
        }
    }

    private IEnumerator HandleSpeedChest(GameObject chest)
    {

        Instantiate(speedEffectPrefab, chest.transform.position, Quaternion.identity);
        chest.GetComponent<Renderer>().enabled = false;
        chest.GetComponent<Collider>().enabled = false;

        currentSpeed = boostedSpeed;
        yield return new WaitForSeconds(2f);
        currentSpeed = moveSpeed;

        yield return new WaitForSeconds(1f);
        chest.GetComponent<Renderer>().enabled = true;
        chest.GetComponent<Collider>().enabled = true;
    }

    private IEnumerator HandleJumpChest(GameObject chest)
    {
        Instantiate(jumpEffectPrefab, chest.transform.position, Quaternion.identity);
        chest.GetComponent<Renderer>().enabled = false;
        chest.GetComponent<Collider>().enabled = false;

        currentJumpForce = boostedJumpForce;
        yield return new WaitForSeconds(3f);
        currentJumpForce = jumpForce;

        yield return new WaitForSeconds(1f);
        chest.GetComponent<Renderer>().enabled = true;
        chest.GetComponent<Collider>().enabled = true;
    }
}