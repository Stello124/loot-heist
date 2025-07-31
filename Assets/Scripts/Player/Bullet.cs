using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 3f;
    public string targetTag = "Enemy";  // Etiketli hedefler için

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Süre sonunda yok et
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Debug.Log("Hedefe çarptý: " + other.name);
            // Hasar, efekt vs burada olur
            Destroy(gameObject);
        }
    }
}
