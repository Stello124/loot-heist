using UnityEngine;

public class PlayerBombToucher : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name} triggered with {other.gameObject.name}");

        if (!other.CompareTag("Player")) return;

        GameObject me = this.gameObject;
        GameObject otherPlayer = other.gameObject;

        if (BombManager.Instance.GetCurrentBombHolder() == me)
        {
            Debug.Log($"{me.name} passed bomb to {otherPlayer.name}");
            BombManager.Instance.SetBombHolder(otherPlayer);
        }
    }
}


