
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerInput : NetworkBehaviour
{
    [SerializeField] private PlayerController1 playerController;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController1>();
        }
    }

    private void Update()
    {
        // Solo el propietario lee su propio teclado
        if (!IsOwner) return;

        if (Keyboard.current == null) return;

        float h = 0f;
        float v = 0f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            h -= 1f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            h += 1f;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            v -= 1f;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            v += 1f;

        Vector2 input = Vector2.ClampMagnitude( new Vector2(h, v), 1f);

        playerController.SendMovementInput(input);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            playerController.RequestJump();

        if (Keyboard.current.cKey.wasPressedThisFrame)
            playerController.RequestColorChange();

        /*if (Keyboard.current.fKey.wasPressedThisFrame)
            playerController.RequestShoot();*/

        if (Keyboard.current.fKey.IsPressed())
            playerController.RequestShoot();
    }
}
