using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyItem : MonoBehaviour
{
    // Texto donde aparece el nombre del jugador
    [SerializeField] private TextMeshProUGUI textName;

    // Imagen que cambia de color según el estado READY
    [SerializeField] private Image readyImage;

    // Referencia al jugador asociado a este elemento
    private PlayeReady player;



    // Establecemos el nombre del jugador
    public void SetPlayerName(string namePlayer)
    {
        textName.text = namePlayer;
    }


    // Conectamos el elemento del Scroll View con el jugador
    public void SetPlayer(PlayeReady newPlayer)
    {
        player = newPlayer;

        // Si es el host, dejamos la imagen siempre verde
        if (player.OwnerClientId == NetworkManager.ServerClientId)
        {
            readyImage.color = Color.green;
            return;
        }

        // Escuchamos los cambios de su estado READY
        player.IsReady.OnValueChanged += OnReadyChanged;

        // Actualizamos el color inicial
        UpdateReady(player.IsReady.Value);
    }


    // Detectamos cuando cambia el estado READY
    private void OnReadyChanged(bool previousValue, bool newValue)
    {
        UpdateReady(newValue);
    }


    // Cambiamos el color de la imagen
    private void UpdateReady(bool ready)
    {
        readyImage.color = ready ? Color.green : Color.red;
    }


    // Eliminamos la suscripción al destruir el elemento
    private void OnDestroy()
    {
        if (player != null)
        {
            player.IsReady.OnValueChanged -= OnReadyChanged;
        }
    }
}