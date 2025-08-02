using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public class BombManager : NetworkBehaviour
{
    public static BombManager Instance;

    public GameObject bombPrefab;
    public float bombTimer = 555f;

    private NetworkVariable<ulong> currentBombHolderClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject bombVisual;
    private Coroutine countdownCoroutine;
    private bool gameStarted = false;

    void Awake()
    {
        Instance = this;
    }

    public void StartBombGame()
    {
        if (!IsServer) return;
        
        gameStarted = true;
        AssignBombToRandomPlayer();
    }
    
    public void OnPlayerCountChanged()
    {
        if (!IsServer || !gameStarted) return;
        
        CheckGameEnd();
    }

    public void AssignBombToRandomPlayer()
    {
        if (!IsServer) return;
        
        var players = NetworkSpawnManager3Map.GetAllNetworkPlayers();
        if (players.Count == 0)
        {
            Debug.LogWarning("💣 Bomba atanacak oyuncu yok!");
            return;
        }
        
        int randomIndex = Random.Range(0, players.Count);
        GameObject selectedPlayer = players[randomIndex];
        
        NetworkObject netObj = selectedPlayer.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            SetBombHolderServerRpc(netObj.OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBombHolderServerRpc(ulong clientId)
    {
        currentBombHolderClientId.Value = clientId;
        UpdateBombVisualClientRpc(clientId);
        
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(BombCountdown());
        
        Debug.Log($"💣 Bomba client {clientId}'ye atandı");
    }
    
    [ClientRpc]
    private void UpdateBombVisualClientRpc(ulong clientId)
    {
        // Önceki bomba görselini temizle
        if (bombVisual != null)
            Destroy(bombVisual);
            
        // Yeni bomba sahibini bul
        GameObject newHolder = GetPlayerByClientId(clientId);
        if (newHolder == null)
        {
            Debug.LogError($"💣 Client {clientId} için oyuncu bulunamadı!");
            return;
        }

        // Karakterin içindeki "RightHand" objesini bul
        Transform hand = newHolder.GetComponentsInChildren<Transform>()
            .FirstOrDefault(t => t.name == "RightHand");

        if (hand == null)
        {
            Debug.LogError("💣 RightHand bulunamadı! Prefabda doğru isimli nesne olduğundan emin ol.");
            return;
        }

        // Bombayı spawn et ve ele yapıştır
        bombVisual = Instantiate(bombPrefab, hand.position, hand.rotation, hand);
        bombVisual.transform.localPosition = Vector3.zero;
        
        Debug.Log($"💣 Bomba görseli {newHolder.name} için oluşturuldu");
    }

    IEnumerator BombCountdown()
    {
        if (!IsServer) yield break;
        
        float time = bombTimer;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        // Bomba patladı - Server'da işle
        ulong explodedClientId = currentBombHolderClientId.Value;
        GameObject explodedPlayer = GetPlayerByClientId(explodedClientId);
        
        if (explodedPlayer != null)
        {
            Debug.Log($"💥 {explodedPlayer.name} patladı! (Client: {explodedClientId})");
            
            // Oyuncuyu yok et
            NetworkObject netObj = explodedPlayer.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn();
            }
        }
        
        // Bomba görselini temizle
        DestroyBombVisualClientRpc();
        currentBombHolderClientId.Value = 0; // Reset

        yield return new WaitForSeconds(1f);

        CheckGameEnd();
    }
    
    [ClientRpc]
    private void DestroyBombVisualClientRpc()
    {
        if (bombVisual != null)
        {
            Destroy(bombVisual);
            bombVisual = null;
        }
    }
    
    private void CheckGameEnd()
    {
        if (!IsServer) return;
        
        var remainingPlayers = NetworkSpawnManager3Map.GetAllNetworkPlayers();
        
        if (remainingPlayers.Count == 1)
        {
            GameObject winner = remainingPlayers[0];
            Debug.Log($"🏆 {winner.name} kazandı!");

            ShowWinnerClientRpc(winner.name);
        }
        else if (remainingPlayers.Count > 1)
        {
            AssignBombToRandomPlayer();
        }
        else
        {
            Debug.Log("🤷 Hiç oyuncu kalmadı!");
        }
    }
    
    [ClientRpc]
    private void ShowWinnerClientRpc(string winnerName)
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowWinText(winnerName);
        }
        Time.timeScale = 0f;
    }

    // Erişim için public getter
    public GameObject GetCurrentBombHolder()
    {
        return GetPlayerByClientId(currentBombHolderClientId.Value);
    }
    
    private GameObject GetPlayerByClientId(ulong clientId)
    {
        if (clientId == 0) return null;
        
        var players = NetworkSpawnManager3Map.GetAllNetworkPlayers();
        foreach (var player in players)
        {
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == clientId)
            {
                return player;
            }
        }
        
        return null;
    }
    
    // Public method for PlayerBombToucher and PlayerAttack to transfer bomb
    public void TransferBombToClient(ulong targetClientId)
    {
        if (!IsServer) return;
        SetBombHolderServerRpc(targetClientId);
    }
}







