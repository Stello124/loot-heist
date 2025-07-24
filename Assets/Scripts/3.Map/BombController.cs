using UnityEngine;

public class BombController : MonoBehaviour
{
    public GameObject bombVisual;   // Bombanýn görünümü
    private bool hasBomb = false;
    private float bombTimer = 15f;

    void Start()
    {
        SetBomb(false); // Baþlangýçta bomba kapalý
    }

    void Update()
    {
        if (hasBomb)
        {
            bombTimer -= Time.deltaTime;

            if (bombTimer <= 0f)
            {
                Explode();
            }
        }
    }

    public void SetBomb(bool value)
    {
        hasBomb = value;
        bombTimer = 15f;
        bombVisual.SetActive(value);
    }

    void Explode()
    {
        Debug.Log(gameObject.name + " PATLADI!");
        Destroy(gameObject); // Oyuncuyu yok et
    }

    public bool HasBomb()
    {
        return hasBomb;
    }
}
