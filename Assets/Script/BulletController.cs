using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{

	[SerializeField] private float speed = 20f;
	[SerializeField] private float lifetime = 3f;

	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			Destroy(gameObject, lifetime);
		}
	}


	private void FixedUpdate()
	{
		if (!IsServer) return;

		transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer) return;

		if(other.TryGetComponent<PlayerController1>(out PlayerController1 hitPlayer))
		{

			hitPlayer.DamageClientRpc();

			gameObject.GetComponent<NetworkObject>().Despawn();
		}
	}

}