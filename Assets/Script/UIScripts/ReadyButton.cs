using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyText;

    private PlayeReady playerReady; //Referencia al script que controla el estado
    private NetworkManager manager;

    [SerializeField] Image buttonBackground; //Fondo del boton

    private void Start()
    {
        manager = NetworkManager.Singleton;

        readyButton.onClick.AddListener(ToggleReady);  //cada vez que pulsemos el boton, cambiara el estado de ready

        manager.OnClientConnectedCallback += OnClientConnected; //nos suscribimos al evento que mira cuando un cliente se a conectado

        // Por si el jugador ya está conectado
        FindLocalPlayer();
    }

    private void OnDestroy() //cuando se destruye el objeto
    {
        readyButton.onClick.RemoveListener(ToggleReady); //eliminamos la asociacion a toggleReady

        if (manager != null)
            manager.OnClientConnectedCallback -= OnClientConnected;

        if (playerReady != null)
            playerReady.IsReady.OnValueChanged -= OnReadyChanged;
    }

    private void OnClientConnected(ulong clientId)//Cuando un cliente se conecta
    {
        FindLocalPlayer(); //Buscamos el jugador local cuando se conecta un cliente
    }

    private void FindLocalPlayer()
    {
        if (manager == null || !manager.IsClient)
            return;

        if (manager.SpawnManager == null)
            return;

        if (manager.SpawnManager.GetLocalPlayerObject() == null)
            return;

        PlayeReady newPlayerReady = manager.SpawnManager.GetLocalPlayerObject().GetComponent<PlayeReady>();//Obtenemos el componente PlayeReady del jugador local

        if (newPlayerReady == null)
            return;

        SetPlayer(newPlayerReady); //asignamos el jugador encontrado
    }

    private void SetPlayer(PlayeReady player) //metodo que conecta el jugador con el boton de ready
    {
        if (playerReady != null)
            playerReady.IsReady.OnValueChanged -= OnReadyChanged;

        playerReady = player; //se guarda la referencia del jugador nuevo

        playerReady.IsReady.OnValueChanged += OnReadyChanged; //nos asociamos a los cambios de la variable "Isready"

        UpdateButton(playerReady.IsReady.Value); //actualizamos el boton segun el estado del jugador
    }

    private void ToggleReady()
    {
        if (playerReady == null)// Comprobamos que el jugador local está asignado
        {
            Debug.LogWarning("PlayerReady todavía no está asignado.");
            return;
        }

        playerReady.ToggleReady();// Llamamos al metodo del jugador para cambiar su estado READY
    }

    private void OnReadyChanged(bool previousValue, bool newValue) //detecta cuando cambia la NetworkVariable IsReady
    {
        UpdateButton(newValue); //actualiza el texto y el color
    }

    private void UpdateButton(bool ready) //actualiza visiblemente el boton
    {
        readyText.text = ready ? "READY" : "NOT READY"; //Si no esta ready, pondra not ready

        buttonBackground.color = ready ? Color.green : Color.red; //si no esta de color verde, estara de color rojo
    }
}