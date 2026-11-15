using System;
using Unity.Netcode;
using UnityEngine;

public class DeathFloor : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player"))
            return;

        KillPlayerRPC(other.transform.root.GetComponent<NetworkObject>());
        
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void KillPlayerRPC(NetworkObjectReference playerReference)
    {
        if (!playerReference.TryGet(out NetworkObject playerObject))
            return;
        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();

        //Si la vida no es null
        if (health != null)
        {
            health.PlayerDied(); 
        }
    }
}
