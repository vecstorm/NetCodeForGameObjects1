using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;

    // Vector de input enviado por el cliente y almacenado en el sevidor
    private Vector2 inputVector;

    // Referencia al componente de fisicas
    private Rigidbody rb;

    private void Awake()
    {
        // asignamos la referencia al componente de fisicas
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(!IsOwner) return; //solo el jugador local controla a su personaje

        float h = Keyboard.current.aKey.IsPressed() ? -1 : 0 + (Keyboard.current.dKey.IsPressed() ? 1:0);
        float v = Keyboard.current.sKey.IsPressed() ? -1 : 0 + (Keyboard.current.wKey.IsPressed() ? 1:0);

        Vector3 move = new Vector3(h, 0, v);

        transform.Translate(move *  speed * Time.deltaTime);
        
    }

    public override void OnNetworkSpawn()
    {

        // Regla clave de fisicas de red:
        // En el servidor, el rigidbody es dinamico (kinematic en false) para simular fisicas
        // El los clientes, el Rigidbody es Cinetico ( kinematic en true) para que la fisica local del cliente
        // no pelee con las posiciones recibidas via NetworkTransform desde el Servidors
        if (IsServer)
        {
            rb.isKinematic = false;
        } else
        {
            rb.isKinematic = true;
        }
    }
}
