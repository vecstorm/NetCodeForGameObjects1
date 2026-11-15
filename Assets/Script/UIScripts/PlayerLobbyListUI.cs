using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerLobbyListUI : MonoBehaviour
{
    [SerializeField] private Transform Content;
    [SerializeField] private GameObject PlayerPrefab;

    private Dictionary<ulong, PlayerLobbyItem> playerItems = new Dictionary<ulong, PlayerLobbyItem>();
    private NetworkManager manager;


    private void OnEnable()
    {
        manager = NetworkManager.Singleton;

        if (manager != null)
        {
            manager.OnClientConnectedCallback += OnPlayerConnected;
            manager.OnClientDisconnectCallback += OnPlayerDisconnected;
        }

        PlayerNameNetwork.OnPlayerNameChanged += RefreshPlayers;
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnClientConnectedCallback -= OnPlayerConnected;
            manager.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }

        PlayerNameNetwork.OnPlayerNameChanged -= RefreshPlayers;
    }

    private void OnPlayerConnected(ulong clientId)
    {
        RefreshPlayers();
    }

    private void OnPlayerDisconnected(ulong clientId)
    {
        RefreshPlayers();
    }

    private void RefreshPlayers()
    {
        NetworkManager manager = NetworkManager.Singleton;

        if (manager == null || !manager.IsListening)
            return;

        foreach (var client in manager.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNameNetwork playerName =
                client.PlayerObject.GetComponent<PlayerNameNetwork>();

            if (playerName == null)
                continue;

            ulong id = client.ClientId;

            if (!playerItems.ContainsKey(id))
            {
                GameObject newPlayer = Instantiate(PlayerPrefab, Content);

                PlayerLobbyItem item = newPlayer.GetComponent<PlayerLobbyItem>();

                playerItems.Add(id, item);
            }

            playerItems[id].SetPlayerName(playerName.PlayerName.Value.ToString()
            );
        }

        // Eliminar jugadores que ya no están conectados.
        List<ulong> disconnectedPlayers = new List<ulong>();

        foreach (var item in playerItems)
        {
            if (!manager.ConnectedClients.ContainsKey(item.Key))
            {
                disconnectedPlayers.Add(item.Key);
            }
        }

        foreach (ulong id in disconnectedPlayers)
        {
            Destroy(playerItems[id].gameObject);
            playerItems.Remove(id);
        }
    }
}
