using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ClientNamePanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField NameInputClient;

    public void ValidateName()
    {
        string playerName = NameInputClient.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            return;

        if (playerName.Length > 20)
            return;

        NetworkManager manager = NetworkManager.Singleton;

        if (manager == null || !manager.IsClient)
        {
            Debug.LogError("El cliente no está conectado.");
            return;
        }

        NetworkObject playerObject =
            manager.LocalClient.PlayerObject;

        if (playerObject == null)
        {
            Debug.LogError("PlayerObject del cliente es NULL.");
            return;
        }

        PlayerNameNetwork playerNameNetwork =
            playerObject.GetComponent<PlayerNameNetwork>();

        if (playerNameNetwork == null)
        {
            Debug.LogError("No existe PlayerNameNetwork.");
            return;
        }

        Debug.Log("Enviando nombre del cliente: " + playerName);

        playerNameNetwork.SubmitPlayerName(playerName);//envia el nombre que introduce el cliente al playerNameNetwork para actualizarlo
    }
}