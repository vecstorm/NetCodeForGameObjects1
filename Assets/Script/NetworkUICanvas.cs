using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System;

public class NetworkUICanvas : MonoBehaviour
{
    [SerializeField] private Button ButtonCreateServer;
    [SerializeField] private Button ButtonJoinServer;

    [SerializeField] private TextMeshProUGUI ServerIPText;
    [SerializeField] private TMP_InputField InputServerIP;

    [SerializeField] private ushort port = 7777;

    [SerializeField] private TMP_InputField InputPlayerName;


    private NetworkManager manager;
    private UnityTransport transport;

    private void Start()
    {
        // Obtenemos el NetworkManager.
        manager = NetworkManager.Singleton;

        if (manager == null)
        {
            Debug.LogError("No se ha encontrado el NetworkManager.");
            return;
        }

        transport = manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("No se ha encontrado UnityTransport.");
            return;
        }

        // Conectamos cada botón con su método.
        ButtonCreateServer.onClick.AddListener(CreateServer);
        ButtonJoinServer.onClick.AddListener(JoinServer);
    }

    private void CreateServer()
    {
        PlayerPrefs.SetString("PlayerName", InputPlayerName.text.Trim());

        transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");

        bool started = manager.StartHost();

        if (started)
        {
            string ip = GetLocalIP();

            ServerIPText.text = "Ip: " + ip + ":" + port;

            Debug.Log("Servidor creado" + ip);
        }
        else
        {
            Debug.LogError("No se ha podido crear el servidor.");
        }
    }

    private string GetLocalIP()
    {
        foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                return ip.ToString();
            }
        }

        return "127.0.0.1";
    }

    private void JoinServer()
    {
        PlayerPrefs.SetString("PlayerName", InputPlayerName.text.Trim());

        string ip = InputServerIP.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("Introduce una IP.");
            return;
        }


        transport.SetConnectionData(ip, port);

        bool connected = manager.StartClient();

        if (!connected)
        {
            Debug.LogError("No se ha podido iniciar el cliente.");
        }
    }

 
}