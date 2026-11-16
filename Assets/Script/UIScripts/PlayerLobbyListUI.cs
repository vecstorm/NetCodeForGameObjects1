
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerLobbyListUI : MonoBehaviour
{
    // ============================================================
    // BLOQUE 1 - CONFIGURACIÓN Y VARIABLES
    // ============================================================

    [Header("Scroll View")]
    [SerializeField] private Transform Content; //Donde se van a spawnear los prefabs del scroll view
    [SerializeField] private GameObject PlayerPrefab; //prefab del jugador

    [Header("Botón de empezar")]
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "Mapa1"; // Nombre de la escena del juego

    // Almacena los prefabs de cada jugador
    private Dictionary<ulong, PlayerLobbyItem> playerItems = new Dictionary<ulong, PlayerLobbyItem>();

    // Guardamos los jugadores a los que ya estamos escuchando
    private Dictionary<ulong, PlayeReady> readyPlayers = new Dictionary<ulong, PlayeReady>();

    private NetworkManager manager;
    private Coroutine initializationCoroutine;


    // ============================================================
    // BLOQUE 2 - INICIALIZACIÓN Y EVENTOS DE RED
    // ============================================================

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

        // Eliminamos las suscripciones a los cambios READY
        foreach (var player in readyPlayers.Values)
        {
            player.IsReady.OnValueChanged -= OnReadyChanged;
        }

        readyPlayers.Clear();
    }

    private void OnPlayerConnected(ulong clientId) //Cuando se conecta un jugador nuevo
    {
        RefreshPlayers();//actualiza la lista del lobby
    }

    private void OnPlayerDisconnected(ulong clientId)//Cuando se desconecta un jugador nuevo
    {
        RefreshPlayers();//actualiza la lista del lobby
    }


    // ============================================================
    // BLOQUE 3 - GESTIÓN DE JUGADORES Y SCROLL VIEW
    // ============================================================

    private void RefreshPlayers()
    {
        if (manager == null || !manager.IsListening)
            return;

        HashSet<ulong> activePlayers = new HashSet<ulong>(); //almacena los identificadores de los jugadores que están en la partida(no se duplican)

        // Recorremos todos los objetos en red
        foreach (NetworkObject networkObject in manager.SpawnManager.SpawnedObjectsList)
        {
            PlayerNameNetwork playerName = networkObject.GetComponent<PlayerNameNetwork>();

            if (playerName == null)
                continue;

            ulong id = networkObject.OwnerClientId;//obtener el identificador del jugador

            activePlayers.Add(id); //se añade a la lista

            // Registramos el estado READY del jugador
            RegisterReadyPlayer(id, networkObject);

            // Creamos su elemento visual si todavía no existe
            if (!playerItems.ContainsKey(id))
            {
                CreatePlayerItem(id, networkObject);
            }

            // Actualizamos el nombre del jugador
            string name = playerName.PlayerName.Value.ToString(); //gurdamos en la variable name el nombre del player que se ha guardado anteriormente

            playerItems[id].SetPlayerName(string.IsNullOrWhiteSpace(name) ? "Conectando..." : name); //mientras que el nombre sea null, muestra conectando
        }

        // Eliminamos los jugadores que ya no están conectados
        RemoveDisconnectedPlayers(activePlayers);

        // Actualizamos el botón después de actualizar la lista
        RefreshStartButton();
    }

    // Registramos el jugador para detectar sus cambios READY
    private void RegisterReadyPlayer(ulong id, NetworkObject networkObject)
    {
        PlayeReady playerReady = networkObject.GetComponent<PlayeReady>();

        // Nos suscribimos a sus cambios de READY una sola vez
        if (playerReady != null && !readyPlayers.ContainsKey(id))
        {
            readyPlayers.Add(id, playerReady);
            playerReady.IsReady.OnValueChanged += OnReadyChanged;
        }
    }

    // Creamos el prefab visual del jugador
    private void CreatePlayerItem(ulong id, NetworkObject networkObject)
    {
        GameObject newPlayer = Instantiate(PlayerPrefab, Content); //lo instancia

        PlayerLobbyItem item = newPlayer.GetComponent<PlayerLobbyItem>(); //Se obtiene el componente de PlayerLobbyItem

        if (item == null)
        {
            Debug.LogError("Falta PlayerLobbyItem en el prefab.");
            Destroy(newPlayer);
            return;
        }

        playerItems.Add(id, item); //Se añade el jugador al diccionario creado

        // Asociamos el elemento visual con el jugador
        PlayeReady playerReady = networkObject.GetComponent<PlayeReady>();

        if (playerReady != null)
        {
            item.SetPlayer(playerReady);
        }
    }

    // Eliminamos los jugadores que se han desconectado
    private void RemoveDisconnectedPlayers(HashSet<ulong> activePlayers)
    {
        List<ulong> playersToRemove = new List<ulong>(); //aqui entraran los identificadores de los jugadores que hay q eliminar

        foreach (var item in playerItems) //recorremos el diccionario
        {
            if (!activePlayers.Contains(item.Key)) //Si el identificador del jugador no esta en la lista
                playersToRemove.Add(item.Key); //lo añadimos a playersToRemove
        }

        foreach (ulong id in playersToRemove) //recorremos la lista de identificadores que queremos eliminar
        {
            // Eliminamos la suscripción al evento READY
            if (readyPlayers.ContainsKey(id))
            {
                readyPlayers[id].IsReady.OnValueChanged -= OnReadyChanged;
                readyPlayers.Remove(id);
            }

            Destroy(playerItems[id].gameObject); //destruimos su gameObject
            playerItems.Remove(id);//y tambien del diccionario
        }
    }


    // ============================================================
    // BLOQUE 4 - CONTROL READY Y BOTÓN DE EMPEZAR
    // ============================================================

    // Se ejecuta cuando cualquier jugador cambia su estado READY
    private void OnReadyChanged(bool previousValue, bool newValue)
    {
        RefreshStartButton();
    }

    // Comprueba si todos los jugadores están READY
    private bool AreAllPlayersReady()
    {
        if (manager == null || !manager.IsListening)
            return false;

        bool allReady = true;
        int playerCount = 0;

        foreach (NetworkObject networkObject in manager.SpawnManager.SpawnedObjectsList)
        {
            PlayerNameNetwork playerName = networkObject.GetComponent<PlayerNameNetwork>();

            if (playerName == null)
                continue;

            playerCount++;

            // El host siempre cuenta como READY
            if (networkObject.OwnerClientId == NetworkManager.ServerClientId)
                continue;

            PlayeReady playerReady = networkObject.GetComponent<PlayeReady>();

            // Si un cliente no está READY, no podemos empezar
            if (playerReady == null || !playerReady.IsReady.Value)
            {
                allReady = false;
                break;
            }
        }

        return playerCount > 0 && allReady;
    }

    // Actualiza el estado del botón EMPEZAR
    private void RefreshStartButton()
    {
        if (manager == null || !manager.IsListening || startButton == null)
            return;

        // Solo el host puede empezar
        startButton.interactable = manager.IsServer && AreAllPlayersReady();
    }


    // ============================================================
    // BLOQUE 5 - INICIO DE LA PARTIDA
    // ============================================================

    // Método que se ejecuta al pulsar el botón EMPEZAR
    public void StartGame()
    {
        // Comprobamos que somos el host
        if (manager == null || !manager.IsServer)
            return;

        // Comprobamos que todos están READY
        if (!AreAllPlayersReady())
            return;

        // Cargamos la escena para todos los jugadores
        manager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }
}
