
using Unity.Netcode;
using UnityEngine;

// Heredamos de NetworkBehaviour en lugar de MonoBehaviour
// porque este objeto necesita comunicarse por red.
public class FadingTile : NetworkBehaviour
{
    
    [SerializeField] private float fadeDuration;// Tiempo que tarda la baldosa en desaparecer


    // Guarda el momento exacto en el que empieza a desaparecer
    // -1 significa que todavía no ha empezado a desaparecer
    // Everyone: todos pueden leer esta variable.
    // Server: solamente el servidor puede modificarla.
    private NetworkVariable<double> fadeStartTime = new NetworkVariable<double>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    /// <summary>
    /// Marca si la baldosa ya no existe
    /// false = la baldosa todavía existe
    ///true = la baldosa ha desaparecido
    /// </summary>
    private NetworkVariable<bool> isGone = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    
    private Renderer[] tileRenderers;// Array que guarda todos los Renderer de la baldosa.
    private Collider tileCollider;
    private Material[][] materials; //Modifican la transparencia del objeto

    void Awake()
    {
        tileRenderers = GetComponentsInChildren<Renderer>();
        tileCollider = GetComponent<Collider>();
        materials = new Material[tileRenderers.Length][];

        // Recorremos todos los Renderer 
        for (int i = 0; i < tileRenderers.Length; i++)
        {
            // Guardamos los materiales de cada Renderer
            materials[i] = tileRenderers[i].materials;
        }
    }


    //Método al que entra cuando el objeto entra en funcionamiento dentro de la red
    public override void OnNetworkSpawn()
    {
        // Comprobamos si la baldosa ya había desaparecido
        if (isGone.Value)
        {
            // Si ya había desaparecido, la ocultamos
            Disappear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto no esta activo y aun no ha desaparecido
        if (!IsSpawned || isGone.Value)
            return;

        // Comprobamos si el objeto que nos toca pertenece a un jugador
        if (!other.transform.root.CompareTag("Player"))
            return;

        //Peticion al servidor para que haga desaparecer la baldosa
        RequestFadeRpc();
    }



    [Rpc(SendTo.Server, RequireOwnership = false)] //Cualquier cliente puede pedir al servidor que haga desaparecer la baldosa
    private void RequestFadeRpc()
    {

        if (isGone.Value || fadeStartTime.Value >= 0)
            return;

        // Guardamos la hora del servidor en la que comienza el desvanecimiento.
        fadeStartTime.Value = NetworkManager.ServerTime.Time;
    }


    void Update()
    {
        // Si el objeto todavía no está activo en la red, no hacemos nada
        if (!IsSpawned)
            return;

        // Si la baldosa ya ha desaparecido, se oculta
        if (isGone.Value)
        {
            Disappear();
            return;
        }

        if (fadeStartTime.Value < 0)
            return;

        //calcula cuánto tiempo ha pasado desde que un jugador a pisado una y lo guarda
        double elapsed = NetworkManager.ServerTime.Time - fadeStartTime.Value;

        //Calcula cuanta transparencia debe tener la baldosa en cualquier momento
        float alpha = Mathf.Clamp01(1f - (float)(elapsed / fadeDuration));


        foreach (Material[] materialArray in materials)
        {
            foreach (Material mat in materialArray)
            {

                // comprobamos si utiliza la propiedad del shader Standard.
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;

                    // Modificamos la transparencia.
                    color.a = alpha;

                    // Aplicamos el nuevo color.
                    mat.color = color;
                }
            }
        }


        // Comprobamos si han pasado los 3 segundos.
        if (elapsed >= fadeDuration && IsServer)
        {
            // Cambiamos el estado de la baldosa(se sincroniza con todos)
            isGone.Value = true;
        }
    }


    // Método que oculta la baldosa
    private void Disappear()
    {
        foreach (Renderer rend in tileRenderers)
        {
            // Desactivamos su representación visual
            rend.enabled = false;
        }

        if (tileCollider != null)
        {
            // Desactivamos la colision
            tileCollider.enabled = false;
        }
    }
}
