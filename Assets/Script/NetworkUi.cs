using Unity.Netcode;
using UnityEngine;

public class NetworkUi : MonoBehaviour
{


    private void OnGUI()
    {
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(20, 20, 200, 60), "Host"))
                NetworkManager.Singleton.StartHost();

            if (GUI.Button(new Rect(20, 100, 200, 60), "Client"))
                NetworkManager.Singleton.StartClient();
        }
    }
}
