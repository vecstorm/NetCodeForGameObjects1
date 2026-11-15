using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerLobbyListUI : MonoBehaviour
{
    [SerializeField] private Transform Content; //Donde se van a spawnear los prefabs del scroll view
    [SerializeField] private GameObject PlayerPrefab; //prefab del jugador

    private Dictionary<ulong, PlayerLobbyItem> playerItems = new Dictionary<ulong, PlayerLobbyItem>(); //Almacena los prefabs

    private NetworkManager manager;

    private Coroutine initializationCoroutine;

    private void OnEnable()
    {
        initializationCoroutine = StartCoroutine(InitializeLobby());
    }

    private IEnumerator InitializeLobby()
    {
        // Esperar a que exista NetworkManager
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }

        manager = NetworkManager.Singleton;

        // Esperar a que la red esté iniciada
        while (!manager.IsListening)
        {
            yield return null;
        }

        Debug.Log("NetworkManager encontrado y conectado.");

        manager.OnClientConnectedCallback += OnPlayerConnected; //nos suscribimos al evento que notifica cuando se conecta un cliente
        manager.OnClientDisconnectCallback += OnPlayerDisconnected;//nos suscribimos al evento que notifica cuando se desconecta un cliente

        PlayerNameNetwork.OnPlayerNameChanged += RefreshPlayers; //se conecta con el script de PlayerNameNetwork

        RefreshPlayers(); //actualiza la lista del lobby
    }

    private void OnDisable()
    {
        if (initializationCoroutine != null) //parar la corrutina
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        if (manager != null) //se eliminan las suscripciones del NetworkManager
        {
            manager.OnClientConnectedCallback -= OnPlayerConnected;
            manager.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }

        PlayerNameNetwork.OnPlayerNameChanged -= RefreshPlayers;
    }

    private void OnPlayerConnected(ulong clientId) //Cuando se conecta un jugador nuevo
    {
        RefreshPlayers();//actualiza la lista del lobby
    }

    private void OnPlayerDisconnected(ulong clientId)//Cuando se desconecta un jugador nuevo
    {
        RefreshPlayers();//actualiza la lista del lobby
    }

    private void RefreshPlayers()
    {
        if (manager == null || !manager.IsListening)
            return;

        HashSet<ulong> activePlayers = new HashSet<ulong>(); //almacena los identificadores de los jugadores que están en la partida(no se duplican)

        foreach (NetworkObject networkObject in manager.SpawnManager.SpawnedObjectsList) //se recorren todos los objetos en red
        {
            PlayerNameNetwork playerName = networkObject.GetComponent<PlayerNameNetwork>();

            if (playerName == null)
                continue;

            ulong id = networkObject.OwnerClientId;//obtener el identificador del jugador

            activePlayers.Add(id); //se añade a la lista

            if (!playerItems.ContainsKey(id)) //si no tiene el prefab de ese objeto instanciado
            {
                GameObject newPlayer = Instantiate(PlayerPrefab, Content); //lo instancia

                PlayerLobbyItem item = newPlayer.GetComponent<PlayerLobbyItem>(); //Se obtiene el componente de PlayerLobbyItem

                if (item == null)
                {
                    Debug.LogError("Falta PlayerLobbyItem en el prefab.");
                    Destroy(newPlayer);
                    continue;
                }

                playerItems.Add(id, item); //Se añade el jugador al diccionario creado
            }

            string name = playerName.PlayerName.Value.ToString(); //gurdamos en la variable name el nombre del player que se ha guardado anteriormente

            playerItems[id].SetPlayerName(string.IsNullOrWhiteSpace(name)? "Conectando...": name); //mientras que el nombre sea null, muestra conectando
        }

        List<ulong> playersToRemove = new List<ulong>(); //aqui entraran los identificadores de los jugadores que hay q eliminar

        foreach (var item in playerItems) //recorremos el diccionario
        {
            if (!activePlayers.Contains(item.Key)) //Si el identificador del jugador no esta en la lista
                playersToRemove.Add(item.Key); //lo añadimos a playersToRemove
        }

        foreach (ulong id in playersToRemove) //recorremos la lista de identificadores que queremos eliminar
        {
            Destroy(playerItems[id].gameObject); //destruimos su gameObject
            playerItems.Remove(id);//y tambien del diccionario
        }
    }
}