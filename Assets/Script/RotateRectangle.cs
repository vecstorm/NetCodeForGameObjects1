using Unity.Netcode;
using UnityEngine;

public class RotateRectangle : NetworkBehaviour
{

    [SerializeField] float rotation = 150;

	private void FixedUpdate()
	{
        if (!IsServer) return;
		startRotate();

	}

	void startRotate()
    {

		transform.Rotate(new Vector3(0, rotation, 0) * Time.fixedDeltaTime);

	}
}
