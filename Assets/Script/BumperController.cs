
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BumperController : NetworkBehaviour
{
	[SerializeField] private float pushSpeed = 8f;
	[SerializeField] private float pushDuration = 0.25f;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer) return;

		PlayerController1 player = other.GetComponentInParent<PlayerController1>();

		if (player == null) return;

		Rigidbody rb = player.GetComponentInChildren<Rigidbody>();

		if (rb == null) return;

		Vector3 direction = player.transform.position - transform.position;

		direction.y = 0f;

		if (direction.sqrMagnitude < 0.001f) return;

		direction.Normalize();

		StartCoroutine(PushPlayer(rb, direction));
	}

	private IEnumerator PushPlayer(Rigidbody rb, Vector3 direction)
	{
		//Buscamos la referencia de PlayerMovement en el rb de alguno de los objetos padre
		PlayerMovement movement = rb.GetComponent<PlayerMovement>();

		//bloqueamos movimiento del player
		if (movement != null)
		{
			movement.SetMovementLocked(true);
		}

		//aplicamos una fuerza al rb y nos esperamos a que acabe el empujon para volver a activar el movimiento
        rb.AddForce(direction * pushSpeed,ForceMode.VelocityChange);
        yield return new WaitForSeconds(pushDuration);

		//Activamos el movimiento de nuevo
        if (movement != null)
		{
			movement.SetMovementLocked(false);
		}
		
	}
}
