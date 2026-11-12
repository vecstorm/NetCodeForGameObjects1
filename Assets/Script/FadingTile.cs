using Unity.Netcode;
using UnityEngine;

public class FadingTile : NetworkBehaviour
{
    [SerializeField] private float fadeDuration = 3f;

    private NetworkVariable<double> fadeStartTime = new NetworkVariable<double>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player"))
            return;

        RequestFadeRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestFadeRpc()
    {
        // Solo el servidor puede iniciar el proceso.
        if (fadeStartTime.Value >= 0)
            return;

        fadeStartTime.Value = NetworkManager.ServerTime.Time;
    }

    void Update()
    {
        if (!IsSpawned || fadeStartTime.Value < 0)
            return;

        double elapsed = NetworkManager.ServerTime.Time - fadeStartTime.Value;

        float alpha = Mathf.Clamp01(1f - (float)(elapsed / fadeDuration));

        // Aplicar el desvanecimiento en todos los jugadores.
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

        if (elapsed >= fadeDuration)
        {
            foreach (Renderer rend in tileRenderers)
                rend.enabled = false;

            if (tileCollider != null)
                tileCollider.enabled = false;
        }
    }
}