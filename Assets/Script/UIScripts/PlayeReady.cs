using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayeReady : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void ToggleReady()
    {
        if (!IsOwner)
            return;

        IsReady.Value = !IsReady.Value;

        Debug.Log("Estado READY: " + IsReady.Value);
    }
}
