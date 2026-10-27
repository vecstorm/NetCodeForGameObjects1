
using UnityEngine;
using Unity.Netcode;

public class NetworkGrabbable : NetworkBehaviour
{
    private Rigidbody rb;

    private Collider[] objectColliders;
    private bool[] originalColliderStates;

    private Collider[] playerColliders;
    private Transform currentPlayerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Obtener todos los colliders del objeto y de sus hijos.
        objectColliders = GetComponentsInChildren<Collider>(true);

        originalColliderStates = new bool[objectColliders.Length];

        for (int i = 0; i < objectColliders.Length; i++)
        {
            originalColliderStates[i] = objectColliders[i].enabled;
        }
    }

    public void Grab(NetworkObject playerNetObj)
    {
        if (!IsServer || playerNetObj == null)
            return;

        if (!NetworkObject.TrySetParent(playerNetObj, false))
        {
            Debug.LogError("[SERVER] No se pudo emparentar el objeto.");
            return;
        }

        currentPlayerTransform = playerNetObj.transform;

        // Obtener los colliders del jugador y sus hijos.
        playerColliders =
            playerNetObj.GetComponentsInChildren<Collider>(true);

        // Ignorar colisiones entre TODOS los colliders de ambos objetos.
        foreach (Collider objectCol in objectColliders)
        {
            if (objectCol == null) continue;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol == null) continue;

                Physics.IgnoreCollision(objectCol, playerCol, true);
            }

            // Desactivar los colliders del objeto mientras se lleva.
            objectCol.enabled = false;
        }

        // Colocar el objeto delante del jugador.
        transform.localPosition = new Vector3(0f, 0f, 1.2f);
        transform.localRotation = Quaternion.identity;

        // Detener la física.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("[SERVER] Objeto cogido correctamente.");
    }

    public void Release(Vector3 throwForce)
    {
        if (!IsServer)
            return;

        // Separar el objeto del jugador.
        if (!NetworkObject.TryRemoveParent())
        {
            Debug.LogWarning("[SERVER] No se pudo quitar el padre.");
            return;
        }

        // Reactivar los colliders con su estado original.
        for (int i = 0; i < objectColliders.Length; i++)
        {
            if (objectColliders[i] != null)
            {
                objectColliders[i].enabled =
                    originalColliderStates[i];
            }
        }

        // Restaurar las colisiones entre el objeto y el jugador.
        if (playerColliders != null)
        {
            foreach (Collider objectCol in objectColliders)
            {
                if (objectCol == null) continue;

                foreach (Collider playerCol in playerColliders)
                {
                    if (playerCol == null) continue;

                    Physics.IgnoreCollision(
                        objectCol, playerCol, false);
                }
            }
        }

        // Reactivar la física y lanzar.
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(throwForce, ForceMode.Impulse);

        currentPlayerTransform = null;
        playerColliders = null;

        Debug.Log("[SERVER] Objeto lanzado correctamente.");
    }
}
