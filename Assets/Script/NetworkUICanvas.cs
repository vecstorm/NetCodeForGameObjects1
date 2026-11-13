using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUICanvas : MonoBehaviour
{
    [SerializeField] private Button ButtonCreateServer;
    [SerializeField] private Button ButtonJoinServer;

    private NetworkManager manager;

    private void Start()
    {
        // Obtenemos el NetworkManager.
        manager = NetworkManager.Singleton;

        if (manager == null)
        {
            Debug.LogError("No se ha encontrado el NetworkManager.");
            return;
        }

        // Conectamos cada botón con su método.
        ButtonCreateServer.onClick.AddListener(CreateServer);
        ButtonJoinServer.onClick.AddListener(JoinServer);
    }

    private void CreateServer()
    {
        manager.StartHost();
    }

    private void JoinServer()
    {
        manager.StartClient();
    }
}