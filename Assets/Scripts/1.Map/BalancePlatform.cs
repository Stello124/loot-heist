using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BalancePlatform : MonoBehaviour
{
    public Transform platform; // Platform model objesi
    public Transform checkAreaCenter;
    public Vector3 checkAreaSize = new Vector3(3f, 1f, 3f);
    public float torqueForce = 10f;
    public float maxAngle = 20f;
    public float angularDrag = 2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularDamping = angularDrag;
    }

    void FixedUpdate()
    {
        // Platformun kendi yönelimini kullanarak kutuyu döndür
        Quaternion rotation = transform.rotation;
        Collider[] hits = Physics.OverlapBox(checkAreaCenter.position, checkAreaSize / 2f, rotation);

        float balance = 0f;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector3 localPos = transform.InverseTransformPoint(hit.transform.position);
                balance += localPos.z;
            }
        }

        float currentAngle = transform.localEulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;

        if (Mathf.Abs(currentAngle) < maxAngle)
        {
            rb.AddTorque(Vector3.back * balance * torqueForce);
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
