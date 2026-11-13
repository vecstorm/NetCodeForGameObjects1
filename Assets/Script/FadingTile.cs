using Unity.Netcode;
using UnityEngine;

public class FadingTile : NetworkBehaviour
{
    [SerializeField] private float fadeDuration = 3f;

    // Momento en el que empieza a desaparecer
    private NetworkVariable<double> fadeStartTime =
        new NetworkVariable<double>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // Indica si la baldosa ya ha desaparecido definitivamente
    private NetworkVariable<bool> isGone =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Renderer[] tileRenderers;
    private Collider tileCollider;
    private Material[][] materials;

    void Awake()
    {
        tileRenderers = GetComponentsInChildren<Renderer>();
        tileCollider = GetComponent<Collider>();

        materials = new Material[tileRenderers.Length][];

        for (int i = 0; i < tileRenderers.Length; i++)
        {
            materials[i] = tileRenderers[i].materials;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (isGone.Value)
        {
            Disappear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSpawned || isGone.Value)
            return;

        if (!other.transform.root.CompareTag("Player"))
            return;

        RequestFadeRpc();
    }

    // El servidor recibe la petición
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestFadeRpc()
    {
        if (isGone.Value || fadeStartTime.Value >= 0)
            return;

        fadeStartTime.Value = NetworkManager.ServerTime.Time;
    }

    void Update()
    {
        if (!IsSpawned)
            return;

        if (isGone.Value)
        {
            Disappear();
            return;
        }

        if (fadeStartTime.Value < 0)
            return;

        double elapsed =
            NetworkManager.ServerTime.Time - fadeStartTime.Value;

        float alpha = Mathf.Clamp01(
            1f - (float)(elapsed / fadeDuration)
        );

        // Todos los jugadores actualizan la transparencia
        foreach (Material[] materialArray in materials)
        {
            foreach (Material mat in materialArray)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color color = mat.GetColor("_BaseColor");
                    color.a = alpha;
                    mat.SetColor("_BaseColor", color);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;
                }
            }
        }

        // Cuando termina el tiempo, el servidor confirma
        // que la baldosa ha desaparecido definitivamente.
        if (elapsed >= fadeDuration && IsServer)
        {
            isGone.Value = true;
        }
    }

    private void Disappear()
    {
        foreach (Renderer rend in tileRenderers)
        {
            rend.enabled = false;
        }

        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }
    }
}