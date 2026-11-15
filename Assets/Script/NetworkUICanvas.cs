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

    [SerializeField] private ushort port = 7777; //Puerto de conexion

    [SerializeField] private TMP_InputField InputPlayerName;


    private NetworkManager manager;
    private UnityTransport transport; //transporte de datos entre dispositivos

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
        PlayerPrefs.SetString("PlayerName", InputPlayerName.text.Trim()); //Guardamos en playerprefs el nombre del jugador (host)

        transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");

        bool started = manager.StartHost(); //Se inicia el modo HOST

        if (started) //si se ha iniciado
        {
            string ip = GetLocalIP(); //gaurda la ip en un string

            ServerIPText.text = "Ip: " + ip + ":" + port; //se imprime por pantalla

            Debug.Log("Servidor creado" + ip);
        }
        else
        {
            Debug.LogError("No se ha podido crear el servidor.");
        }
    }

    private string GetLocalIP() //metodo para sacar la IP local
    {
        foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) //recorremos todas las direcciones IP de nuestro PC
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)) //buscamos una dirección IPv4 que no sea de loopback
            {
                return ip.ToString();
            }
        }

        return "127.0.0.1";
    }

    private void JoinServer() //metodo para unirte al servidor
    {
        PlayerPrefs.SetString("PlayerName", InputPlayerName.text.Trim()); //guardamos el nombre del jugador (clientes)
        PlayerPrefs.Save();

        string ip = InputServerIP.text.Trim(); //se obtiene la ip que ha introducido el cliente eliminando espacios

        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("Introduce una IP.");
            return;
        }

        transport.SetConnectionData(ip, port); //configuramos el transport para que se conecte a la ip que ha puesto el jugador

        bool connected = manager.StartClient(); //se inicia el cliente

        if (!connected)
        {
            Debug.LogError("No se ha podido iniciar el cliente.");
        }
    }


}