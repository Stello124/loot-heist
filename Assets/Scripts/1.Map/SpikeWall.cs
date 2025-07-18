using UnityEngine;

public class SpikeWall : MonoBehaviour
{
    [Header("Positions")]
    public Transform openPosition;      // Açık pozisyon
    public Transform closedPosition;    // Kapalı pozisyon

    [Header("Movement Settings")]
    public float closeSpeed = 10f;      // Kapanma hızı
    public float openSpeed = 2f;        // Açılma hızı
    public float waitTime = 2f;         // Her hareketten sonra bekleme

    [Header("Start Settings")]
    public float startDelay = 0f;       // Başlamadan önce bekleme süresi (her obje için farklı olabilir)

    private bool isClosing = true;
    private float timer;
    private bool started = false;

    void Start()
    {
        // Başlama gecikmesini başlat
        Invoke(nameof(StartMovement), startDelay);
    }

    void StartMovement()
    {
        started = true;
    }

    void Update()
    {
        if (!started) return;

        timer += Time.deltaTime;

        if (timer >= waitTime)
        {
            if (isClosing)
            {
                transform.position = Vector3.MoveTowards(transform.position, closedPosition.position, closeSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, closedPosition.position) < 0.01f)
                {
                    isClosing = false;
                    timer = 0f;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, openPosition.position, openSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, openPosition.position) < 0.01f)
                {
                    isClosing = true;
                    timer = 0f;
                }
            }
        }
    }
}
