using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    public int playerCount = 4;

    public static List<GameObject> allPlayers = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < playerCount; i++)
        {
            int spawnIndex = i % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnIndex];

            GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.name = "Player" + (i + 1);
            allPlayers.Add(player);
        }

        BombManager.Instance.AssignBombToRandomPlayer();
    }
}







