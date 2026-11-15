using Unity.Netcode;
using Unity.Collections;
using System;

public class PlayerNameNetwork : NetworkBehaviour
{
    public static event Action OnPlayerNameChanged;

    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnNameChanged;

        OnPlayerNameChanged?.Invoke();
    }

    public void SubmitPlayerName(string playerName)
    {
        if (!IsOwner)
            return;

        SetPlayerNameServerRpc(playerName);
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return;

        if (playerName.Length > 20)
            return;

        PlayerName.Value = playerName.Trim();
    }

    private void OnNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        OnPlayerNameChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnNameChanged;
        OnPlayerNameChanged?.Invoke();
    }
}