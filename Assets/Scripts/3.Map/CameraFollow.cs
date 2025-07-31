using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static Transform target;
    public Vector3 offset = new Vector3(0, 5, -6);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target);
    }

    public static void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
