using UnityEngine;

public class BombGameManager : MonoBehaviour
{
    void Start()
    {
        BombController[] players = FindObjectsOfType<BombController>();
        int randomIndex = Random.Range(0, players.Length);
        players[randomIndex].SetBomb(true);
    }
}

