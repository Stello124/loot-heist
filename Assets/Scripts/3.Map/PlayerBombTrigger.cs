using UnityEngine;

public class PlayerBombTrigger : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (!BombManager.instance) return;

        if (BombManager.instance.currentBomber == gameObject)
        {
            BombManager.instance.TransferBomb(other.gameObject);
        }
    }
}

