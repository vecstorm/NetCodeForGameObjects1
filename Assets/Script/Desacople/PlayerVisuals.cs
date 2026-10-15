
using System.Collections;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Renderer playerRenderer;

    private Coroutine damageFlashCoroutine;

    private void Awake()
    {
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<Renderer>();
        }
    }

    public void SetColor(Color color)
    {
        if (playerRenderer == null) return;

        playerRenderer.material.color = color;
    }

    public void PlayDamageFlash(Color originalColor)
    {
        if (playerRenderer == null) return;

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        damageFlashCoroutine =
            StartCoroutine(DamageFlashCoroutine(originalColor));
    }

    private IEnumerator DamageFlashCoroutine(Color originalColor)
    {
        playerRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        playerRenderer.material.color = originalColor;

        damageFlashCoroutine = null;
    }
}
