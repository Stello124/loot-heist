using System.Collections.Generic;
using System.Net.Security;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GetLobbies : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GameObject buttonsContainer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void GetLobbiesTest()
    {
        ClearContainer();
        try
        {
            QueryLobbiesOptions options = new();
            Debug.LogWarning("QueryLobbiesTest");
            
            options.Count= 25;

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                
            };


            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(true, QueryOrder.FieldOptions.Created)
            };



            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            Debug.LogWarning("Get Lobbies Done COUNT"+lobbies.Results.Count);
            foreach (Lobby bulunanLobby in lobbies.Results)
            {
                LobbyStatic.LogLobby(bulunanLobby);
                CreateLobbyButton(bulunanLobby);
            }
            
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    private void CreateLobbyButton(Lobby lobby)
    {
        var button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
        button.name =lobby.Name+"_button";
        button.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Name;
        var recTransform = button.GetComponent<RectTransform>();
        recTransform.SetParent(buttonsContainer.transform);
        button.GetComponent<Button>().onClick.AddListener(delegate () { Lobby_OnClick(lobby); });
    }
    public void Lobby_OnClick(Lobby lobby)
    {
        Debug.Log("Clicked Lobby :"+lobby.Name);
        GetComponent<JoinLobby>().JoinLobbyWithLobbyId(lobby.Id);
    }
    private void ClearContainer()
    {
        if (buttonsContainer != null && buttonsContainer.transform.childCount >0)
        {
            foreach (Transform VARIABLE in buttonsContainer.transform)
            {
                Destroy(VARIABLE.gameObject);
            }
        }
    }

}
