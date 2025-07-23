using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public static BombManager instance;

    public float bombTimer = 15f;
    private float currentTimer;

    public GameObject currentBomber; // Bombayý taþýyan oyuncu

    private bool gameRunning = false;
    private List<GameObject> alivePlayers = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    public void StartGame(List<GameObject> players)
    {
        alivePlayers = new List<GameObject>(players);
        PickRandomBomber();
        currentTimer = bombTimer;
        gameRunning = true;
    }

    void Update()
    {
        if (!gameRunning || currentBomber == null) return;

        currentTimer -= Time.deltaTime;

        if (currentTimer <= 0f)
        {
            PlayerExplode(currentBomber);
        }
    }

    public void TransferBomb(GameObject newBomber)
    {
        if (newBomber == currentBomber) return;

        // Eski bomber'ý durdur
        var oldVisual = currentBomber.GetComponent<PlayerVisual>();
        if (oldVisual != null)
            oldVisual.SetBomb(false);

        // Yeni bomber'ý ata
        currentBomber = newBomber;
        var newVisual = currentBomber.GetComponent<PlayerVisual>();
        if (newVisual != null)
            newVisual.SetBomb(true);

        currentTimer = bombTimer;
    }

    void PickRandomBomber()
    {
        int rand = Random.Range(0, alivePlayers.Count);
        currentBomber = alivePlayers[rand];
        var visual = currentBomber.GetComponent<PlayerVisual>();
        if (visual != null)
            visual.SetBomb(true);
    }

    void PlayerExplode(GameObject player)
    {
        alivePlayers.Remove(player);
        Destroy(player); // Oyuncuyu sahneden kaldýr

        if (alivePlayers.Count == 1)
        {
            EndGame(alivePlayers[0]);
        }
        else
        {
            PickRandomBomber();
            currentTimer = bombTimer;
        }
    }

    void EndGame(GameObject winner)
    {
        gameRunning = false;
        Debug.Log(winner.name + " kazandý!");
        // Buraya istersen patlama efekti veya oyun sonu iþlemi ekleyebilirsin
    }
}


