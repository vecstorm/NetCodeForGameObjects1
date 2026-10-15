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



    private void Awake()
    {
        //obtenemos la referncia del RB del GameObject
        rb = GetComponent<Rigidbody>();
    }

    //REcibe llos datos de los inputs pero no lee el teclado
    //Nos indica la dirección de movimiento
    public void SetInput(Vector2 input)
    {
        //Hacemos que las diagonales no sean mas rapidas y limitamos la magnitud de entrada a 1
        inputVector = Vector2.ClampMagnitude(input, 1f);

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
        //Si este jugador no simula física, no hacemos nada
        if (!simulationEnabled) return;
		if (movementLocked) return;

		//lanzamos un rayo hacia abajo para comprobar el suelo
		isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundLayer);

        // convertimos la entrada 2d en una dirección 3d
        // x controla el movimiento saltera y z el avance
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        //convertimos la dirección en una velocidad horizontal
        Vector3 targetVelocity = moveDirection * speed;
        
        //Modificamos X y Z, pero conservamos la velocidad Y
        //No anulamos la gravedad ni la caida del jugador
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // si hay movimiento orientamos al jugador hacia la dirección en la que se desplaza
        if (moveDirection.sqrMagnitude > 0.01f) 
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

}
