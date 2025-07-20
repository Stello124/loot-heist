using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 1f; // Ulaştığında kaç saniye beklesin

    private Transform target;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                target = (target == pointA) ? pointB : pointA;
            }
            return; // Bekliyorken hareket etmesin
        }

        // Hedefe doğru hareket
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Hedefe ulaştıysa beklemeye başla
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }
}
