using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 1f;

    private Transform target;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            // Eğer pointA ve B atanmadıysa, objenin altındaki çocuklardan al
            pointA = transform.Find("PointA");
            pointB = transform.Find("PointB");

            if (pointA == null || pointB == null)
            {
                Debug.LogError("PointA ve/veya PointB atanmadı ve obje içinde bulunamadı!");
                enabled = false;
                return;
            }
        }

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
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }
}
