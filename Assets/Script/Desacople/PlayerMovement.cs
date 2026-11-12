using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private Vector2 inputVector;
    private Rigidbody rb;
    private bool isGrounded;
    private bool simulationEnabled;

    private bool movementLocked = false;

    [SerializeField] Transform camera;
    private float cameraYaw;


    private void Awake()
    {
        //obtenemos la referncia del RB del GameObject
        rb = GetComponent<Rigidbody>();
    }

    //REcibe llos datos de los inputs pero no lee el teclado
    //Nos indica la dirección de movimiento
    public void SetInput(Vector2 input, float yaw)
    {
        //Hacemos que las diagonales no sean mas rapidas y limitamos la magnitud de entrada a 1
        inputVector = Vector2.ClampMagnitude(input, 1f);
        cameraYaw = yaw;

    }

    //Permite activarr o desactivas las fisicas
    // la red decidira cuando se tiene que activar
    public void SetSimulationEnabled(bool enabled)
    {
        simulationEnabled = enabled;
    }

    //Solicitamos un salto
    public void TryJump()
    {
        Debug.Log(
    $"TryJump: simulationEnabled={simulationEnabled}, " +
    $"isGrounded={isGrounded}"
);

        // No saltamos si la fisica esta desactivada o si el jugador no esta en el suelo
        if (!simulationEnabled || !isGrounded) return;

        //aplicamos un impulso instantaneo hacia arriba
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked) 
        {
            inputVector = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!simulationEnabled) return;
        if (movementLocked) return;

        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundLayer);
        //1.  creamos una rotación utilizando el ángulo horizontal de la cámara
        Quaternion cameraRotation = Quaternion.Euler(0f, cameraYaw, 0f);

        // 2. Obtenemos las direcciones relativas a la cámara
        Vector3 cameraForward = cameraRotation * Vector3.forward;
        Vector3 cameraRight = cameraRotation * Vector3.right;

        // 3. Combinamos ambas direcciones con el input
        Vector3 moveDirection = cameraForward * inputVector.y + cameraRight * inputVector.x;

        // 5. Ajustamos parta que el movimiento diagonal no sea mas rápido que el movimiento en línia recta
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        Vector3 targetVelocity = moveDirection * speed;

        rb.linearVelocity = new Vector3( targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (moveDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(moveDirection);
    }

}
