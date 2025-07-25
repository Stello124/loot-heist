using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        // Etki göstermek istiyorsan buraya ekleyebilirsin
        Destroy(gameObject);
    }
}
