using UnityEngine;

public class BombPasser : MonoBehaviour
{
    public bool HasBomb = false;
    private GameObject bombVisual;

    private void Start()
    {
        bombVisual = transform.Find("BombVisual")?.gameObject;
        if (bombVisual != null)
            bombVisual.SetActive(false);
    }

    public void ReceiveBomb()
    {
        HasBomb = true;
        if (bombVisual != null)
            bombVisual.SetActive(true);
    }

    public void RemoveBomb()
    {
        HasBomb = false;
        if (bombVisual != null)
            bombVisual.SetActive(false);
    }

    public void Explode()
    {
        Debug.Log($"{gameObject.name} patladý!");
        Destroy(gameObject); // Patlama efektiyle deðiþtirilebilir
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasBomb) return;

        BombPasser other = collision.gameObject.GetComponent<BombPasser>();
        if (other != null && !other.HasBomb)
        {
            RemoveBomb();
            other.ReceiveBomb();
        }
    }
}
