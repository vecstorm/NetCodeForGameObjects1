using Unity.Netcode;
using UnityEngine;
using TMPro;

public class HostNamePanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField NameInputHost;

    public void ValidateName()//metodo que valida el nombre del host 
    {
        string playerName = NameInputHost.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Introduce un nombre.");
            return;
        }

        if (playerName.Length > 20)
        {
            Debug.LogWarning("Máximo 20 caracteres.");
            return;
        }

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (playerObject == null)
        {
            Debug.LogError("No se ha encontrado el jugador del host.");
            return;
        }

        PlayerNameNetwork playerNameNetwork =playerObject.GetComponent<PlayerNameNetwork>();

        playerNameNetwork.SubmitPlayerName(playerName);//envia el nombre que introduce el host al playerNameNetwork para actualizarlo

    }
}