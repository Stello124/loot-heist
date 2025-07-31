using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    private string PlayerID;
    private RelayHostData _hostData;
    private RelayJoinData _joinData;

    public TMP_InputField inputField;
    public TextMeshProUGUI IdText;
    public TextMeshProUGUI JoinCodeText;
    public TMP_Dropdown playerCount;

    public GameObject playerPrefab;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("✅ Unity Services Initialized");
        SignIn();
    }

    async void SignIn()
    {
        Debug.Log("🔐 Signing in...");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        PlayerID = AuthenticationService.Instance.PlayerId;
        Debug.Log("🆔 Signed in as: " + PlayerID);
        IdText.text = PlayerID;
    }

    public async void OnHostClick()
    {
        int maxPlayerCount = Convert.ToInt32(playerCount.options[playerCount.value].text);

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayerCount);
        _hostData = new RelayHostData()
        {
            IPv4Address = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            Key = allocation.Key,
        };
        _hostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(_hostData.AllocationID);
        Debug.Log("✅ Relay Allocation Complete: " + _hostData.AllocationID);
        Debug.LogWarning("📎 JoinCode = " + _hostData.JoinCode);
        JoinCodeText.text = _hostData.JoinCode;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_hostData.IPv4Address, _hostData.Port, _hostData.AllocationIDBytes, _hostData.Key, _hostData.ConnectionData);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.StartHost();
        Debug.Log("🚀 Host started");

        SpawnPlayer(); // Host kendi karakterini spawn eder
    }

    public async void OnJoinClick()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            Debug.LogError("❌ Join Code boş! Client bağlanamaz.");
            return;
        }

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(inputField.text);
        _joinData = new RelayJoinData()
        {
            IPv4Address = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            Key = allocation.Key,
        };
        Debug.Log("✅ Relay Join Success: " + _joinData.AllocationID);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_joinData.IPv4Address, _joinData.Port, _joinData.AllocationIDBytes, _joinData.Key, _joinData.ConnectionData, _joinData.HostConnectionData);

        NetworkManager.Singleton.StartClient();
        Debug.Log("🔗 Client started and attempting to connect");
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log("🎮 Client connected: " + clientId);

        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(); // Server tüm client'lar için spawn yapar
        }
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ Player prefab is not assigned!");
            return;
        }

        var playerObj = Instantiate(playerPrefab);
        var netObj = playerObj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ Player prefab does not have a NetworkObject component!");
            return;
        }

        netObj.Spawn();
        Debug.Log("✅ Player spawned");
    }
}

public struct RelayHostData
{
    public string JoinCode;
    public string IPv4Address;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
}

public struct RelayJoinData
{
    public string IPv4Address;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] Key;
}