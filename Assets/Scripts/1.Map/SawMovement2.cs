using UnityEngine;

public class SawMovement2 : MonoBehaviour
{
    public float moveDistance = 3f; // Sağ-sola ne kadar gitsin
    public float moveSpeed = 2f;    // Hızı

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + new Vector3(offset, 0f, 0f); // Z ekseninde ileri geri
    }
}
