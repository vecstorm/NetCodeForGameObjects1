using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Collections;
using Unity.Netcode.Components;
using System.Collections;

public class PlayerController1 : NetworkBehaviour
{
    //new
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerVisuals visuals;



    [Header("Movement")]
    [SerializeField] private float speed = 5f;


    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Player Appearance")]
    [SerializeField] private Renderer playerRenderer;

    [SerializeField] private GameObject bullet;

    [SerializeField] private GameObject spawnPoint;

    [SerializeField] GameObject text;

	private NetworkGrabbable nearbyObject;
	private NetworkGrabbable currentHeldObject;


	// NetworkVariable que almacena el color del jugador
	private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // NetworkVariable que almacena el nombre del jugador
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes("Jugador"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

   
    private Rigidbody rb;
    [SerializeField] private float pushForce;

    private void Awake()
    {
        //Asignación de referencia al componente de fisicas y al componente de audio
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<Renderer>();
        }

        //En caso que no hayamos establecido el PlayerMovement en el inspector, unity lo busca automaticamente 
        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }

        if (visuals == null)
        {
            visuals = GetComponent<PlayerVisuals>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //activamos el movimiento en el servidor
        movement.SetSimulationEnabled(IsServer);



        // Suscripción a los eventos de cambio de las NetworkVariables
        playerColor.OnValueChanged += OnPlayerColorChanged;
        playerName.OnValueChanged += OnPlayerNameChanged;

        // El servidor establece los valores iniciales de las NetworkVariables
        if (IsServer)
        {
            // Color aleatorio al conectarse
            playerColor.Value = Random.ColorHSV();

            // Nombre por defecto: Jugador + ID cliente
            playerName.Value = new FixedString32Bytes("Jugador " + OwnerClientId);
        }

        // Aplicar el color actual al aparecer en red
        ApplyColor(playerColor.Value);

        // REGLA CLAVE DE FISICA EN RED: 
        // En el Server, el Rigidbody es Dinamico (isKinematic = false) para simular fisicas
        // En los Clientes, el Rigidbody es Cinematico ( is kinematic = true) para que la fisica local del cliente
        // no pelee con las posiciones recibidas via NetworkTransform desde el servidor
        if (IsServer)
        {
            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        // Desuscribirse de los eventos de cambio de las NetworkVariables
        playerColor.OnValueChanged -= OnPlayerColorChanged;
        playerName.OnValueChanged -= OnPlayerNameChanged;

        base.OnNetworkDespawn();
    }


    [ServerRpc]
	private void RequestShootServerRpc()
	{

            GameObject newBullet = Instantiate(bullet, spawnPoint.transform.position, spawnPoint.transform.rotation);
            newBullet.GetComponent<NetworkObject>().Spawn();

	}

	[ServerRpc]
	private void RequestChangeWeaponServerRpc()
	{
		
	}

	/// <summary>
	/// ServerRpc: Método ejecutado exclusivamente en el servidor tras ser invocado por el cliente
	/// El sufijo "ServerRpc" y el atributo [ServerRpc] son obligatorios en Unity Netcode for GameObjects.
	/// </summary>
	/// <param name="input"></param>
	[ServerRpc]
    private void SubmitInputServerRpc(Vector2 input, float cameraYAw)
    {
        // El servidor recibe la dirección pedida por el cliente
        movement.SetInput(input, cameraYAw);
    }

    [ServerRpc]
    private void RequestJumpServerRpc()
    {
        
        movement.TryJump();

        PlayJumpSoundClientRpc();
    }

    // ServerRpc utilizado para solicitar al servidor un nuevo color
    [ServerRpc]
    private void RequestColorChangeServerRpc()
    {
        playerColor.Value = Random.ColorHSV();
    }

    [ClientRpc]
    private void PlayJumpSoundClientRpc()
    {
        audioSource.Stop();
        audioSource.Play();
    }

    // Se ejecuta cuando cambia la NetworkVariable que controla el color
    private void OnPlayerColorChanged(Color previousColor, Color newColor)
    {
        ApplyColor(newColor);
    }

    // Aplica el color recibido al Renderer de la cápsula
    private void ApplyColor(Color color)
    {

        if (visuals != null)
        {
            visuals.SetColor(color);
        }
    }

    // Se ejecuta cuando cambia la NetworkVariable que controla el nombre
    private void OnPlayerNameChanged(
        FixedString32Bytes previousName,
        FixedString32Bytes newName)
    {
        Debug.Log("Nombre del jugador cambiado a: " + newName.ToString());
    }

    [ClientRpc]
    public void DamageClientRpc()
    {
        Vector3 pushDirection = rb.transform.forward;
        if (visuals != null) 
        {
            visuals.PlayDamageFlash(playerColor.Value);
        }
		rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
	}

	public void RequestInteract()
	{
		// Si no llevamos nada y hay un objeto cerca -> Coger
		if (currentHeldObject == null && nearbyObject != null)
		{
			Debug.Log("[INTERACT] Intentando COGER objeto...");
			currentHeldObject = nearbyObject;
			RequestGrabServerRpc(currentHeldObject.GetComponent<NetworkObject>());

			nearbyObject = null;
			if (text != null) text.SetActive(false);
		}
		// Si ya llevamos un objeto -> Lanzar
		else if (currentHeldObject != null)
		{
			Debug.Log("[INTERACT] Intentando LANZAR objeto...");
			// Calculamos la fuerza hacia adelante del jugador
			Vector3 throwForceDir = transform.forward * 10f + Vector3.up * 2f; // Impulso hacia adelante y un poco hacia arriba
			RequestReleaseServerRpc(currentHeldObject.GetComponent<NetworkObject>(), throwForceDir);

			currentHeldObject = null;
        }
        else
        {
			Debug.Log("[INTERACT] No hay objeto cerca ni objeto en mano.");
		}
	}

	[ServerRpc]
	private void RequestGrabServerRpc(NetworkObjectReference objRef)
	{
		if (objRef.TryGet(out NetworkObject netObj))
		{
			if (netObj.TryGetComponent<NetworkGrabbable>(out NetworkGrabbable grabbable))
			{
				grabbable.Grab(GetComponent<NetworkObject>());
			}
		}
	}

	[ServerRpc]
	private void RequestReleaseServerRpc(NetworkObjectReference objRef, Vector3 force)
	{
		if (objRef.TryGet(out NetworkObject netObj))
		{
			if (netObj.TryGetComponent<NetworkGrabbable>(out NetworkGrabbable grabbable))
			{
				grabbable.Release(force);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<NetworkGrabbable>(out NetworkGrabbable grabbable))
		{
			nearbyObject = grabbable;
			text.SetActive(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<NetworkGrabbable>(out NetworkGrabbable grabbable))
		{
			if (nearbyObject == grabbable)
			{
				nearbyObject = null;
				text.SetActive(false);
			}
		}
	}



	public void SendMovementInput(Vector2 input, float cameraYaw)
    {
        SubmitInputServerRpc(input, cameraYaw);
    }

    public void RequestJump()
    {
        RequestJumpServerRpc();
    }

    public void RequestColorChange()
    {
        RequestColorChangeServerRpc();
    }

    public void RequestShoot()
    {
        RequestShootServerRpc();
    }

}
