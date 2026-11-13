
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void PlayerDied() //(este metodo se usa en DeathFloor -> script)
    {
        // Solo el servidor puede matar al jugador.
        if (!IsServer || isDead.Value)
            return;

        isDead.Value = true;
        Debug.Log(OwnerClientId + " ha muerto.");

        // Desactivamos el movimiento del jugador.
        // Aquí añadiremos después tu sistema de movimiento.
    }
}
