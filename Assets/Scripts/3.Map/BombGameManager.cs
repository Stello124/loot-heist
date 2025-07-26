using System.Collections.Generic;
using UnityEngine;

public class BombGameManager : MonoBehaviour
{
    public List<GameObject> players = new List<GameObject>();
    private float roundTimer = 15f;
    private float timer;

    private void Start()
    {
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        // Rastgele bir oyuncuya bombayý ver
        int randomIndex = Random.Range(0, players.Count);
        players[randomIndex].GetComponent<BombPasser>().ReceiveBomb();
        timer = roundTimer;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            foreach (GameObject player in players)
            {
                BombPasser passer = player.GetComponent<BombPasser>();
                if (passer != null && passer.HasBomb)
                {
                    passer.Explode();
                    break;
                }
            }
        }
    }
}

