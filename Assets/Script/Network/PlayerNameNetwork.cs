using Unity.Netcode;
using Unity.Collections;
using System;
using UnityEngine;

public class PlayerNameNetwork : NetworkBehaviour
{
    public static event Action OnPlayerNameChanged; //Avisa a otros scripts que el nombre a cambiado para actualizar textos...

    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnNameChanged; //Queremos ver cuando se cambia el nombre del jugador

        OnPlayerNameChanged?.Invoke();

        if (IsOwner) //Miramos si el objeto es del propio jugador local
        {
            string savedName = PlayerPrefs.GetString("PlayerName", ""); //Recogemos el nombre que se ha guardado en NetworkUICanvas

            if (!string.IsNullOrWhiteSpace(savedName))
            {
                Debug.Log("Enviando nombre guardado: " + savedName);

                SubmitPlayerName(savedName);
            }
            else
            {
                Debug.LogWarning("No hay ningún nombre guardado.");
            }
        }
    }

    public void SubmitPlayerName(string playerName)
    {
        Debug.Log("Nombre: " + playerName + ", IsOwner: " + IsOwner);

        if (!IsOwner)
            return;

        SetPlayerNameServerRpc(playerName); //Enviamos el nombre al servidor
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string playerName)
    {
        Debug.Log("Jugador: " + OwnerClientId + " Nombre: " + playerName);

        if (string.IsNullOrWhiteSpace(playerName)) //Validamos el nombre
            return;

        if (playerName.Length > 20) //que no sea mayor a 20 digitos
            return;

        PlayerName.Value = playerName.Trim(); //Aqui se actualiaz el nombre, ya que el servidor modifica el valor en la NetworkVariable
    }

    private void OnNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName) //se ejecuta automaticamente cuando cambia el valor de PlayerName
    {
        OnPlayerNameChanged?.Invoke();
    }

    public override void OnNetworkDespawn() //se ejecuta cuando el objeto de red desaparece de la sesión
    {
        PlayerName.OnValueChanged -= OnNameChanged;
        OnPlayerNameChanged?.Invoke();
    }
}