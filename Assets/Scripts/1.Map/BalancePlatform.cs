using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BalancePlatform : MonoBehaviour
{
    public Transform platform;
    public Transform checkAreaCenter;
    public Vector3 checkAreaSize = new Vector3(3f, 1f, 3f);
    public float torqueForce = 10f;
    public float maxAngle = 20f;
    public float angularDrag = 2f;
    public float resetDelay = 2f; // Kaç saniye sonra sıfıra dönsün
    public float resetSpeed = 5f; // Sıfıra dönüş hızı

    private Rigidbody rb;
    private float noPlayerTimer = 0f;
    private bool hasPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularDamping = angularDrag;
    }

    void FixedUpdate()
    {
        Quaternion rotation = transform.rotation;
        Collider[] hits = Physics.OverlapBox(checkAreaCenter.position, checkAreaSize / 2f, rotation);

        float balance = 0f;
        hasPlayer = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector3 localPos = transform.InverseTransformPoint(hit.transform.position);
                balance += localPos.z;
                hasPlayer = true;
            }
        }

        float currentAngle = transform.localEulerAngles.x;
        if (currentAngle > 180f) currentAngle -= 360f;

        if (hasPlayer)
        {
            noPlayerTimer = 0f;

            if (Mathf.Abs(currentAngle) < maxAngle)
                rb.AddTorque(Vector3.back * balance * torqueForce);
        }
        else
        {
            noPlayerTimer += Time.fixedDeltaTime;

            if (noPlayerTimer >= resetDelay)
            {
                float angleDiff = -currentAngle; // 0'a dönmesi için
                float torque = angleDiff * resetSpeed;
                rb.AddTorque(Vector3.back * torque);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (checkAreaCenter == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = checkAreaCenter.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, checkAreaSize);
    }
}
