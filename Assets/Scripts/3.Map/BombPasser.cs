using UnityEngine;

public class BombPasser : MonoBehaviour
{
    private BombController bombController;

    void Start()
    {
        bombController = GetComponent<BombController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!bombController.HasBomb()) return;

        BombController otherBomb = collision.gameObject.GetComponent<BombController>();

        if (otherBomb != null && !otherBomb.HasBomb())
        {
            bombController.SetBomb(false);
            otherBomb.SetBomb(true);
        }
    }
}

