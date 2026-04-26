using System.Collections;
using UnityEngine;

/// <summary>
/// Mur de cachette dans le mini-jeu Cache-cache.
/// Quand l'IA utilise sa competence speciale, cet obstacle glow puis monte vers le haut et disparait.
/// Taggez ces GameObjects avec "HidingObstacle" dans l'Inspector.
/// </summary>
public class HidingObstacle : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("Duree de la montee progressive du glow avant de partir (secondes).")]
    public float glowDuration = 0.6f;
    [Tooltip("Couleur du glow. Doit correspondre a la couleur d'emission du materiau.")]
    public Color glowColor = new Color(1f, 0.4f, 0f);
    [Tooltip("Intensite maximale de l'emission HDR.")]
    public float glowMaxIntensity = 4f;

    [Header("Fly Away Settings")]
    [Tooltip("Hauteur totale montee avant disparition (metres).")]
    public float flyHeight = 8f;
    [Tooltip("Duree du vol vers le haut (secondes).")]
    public float flyDuration = 1.2f;

    private Renderer[] renderers;
    private bool isRemoving;

    // Registre statique de tous les obstacles actifs de la scene.
    private static readonly System.Collections.Generic.List<HidingObstacle> activeObstacles
        = new System.Collections.Generic.List<HidingObstacle>();

    public static int ActiveCount => activeObstacles.Count;

    private void OnEnable()
    {
        if (!activeObstacles.Contains(this))
            activeObstacles.Add(this);
    }

    private void OnDisable()
    {
        activeObstacles.Remove(this);
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Retourne un obstacle actif aleatoire et le reserve immediatement (retire de la liste).
    /// Garantit qu'une autre IA ne peut pas choisir le meme obstacle au meme frame.
    /// Retourne null s'il n'en reste plus.
    /// </summary>
    public static HidingObstacle ClaimRandom()
    {
        if (activeObstacles.Count == 0) return null;
        int index = Random.Range(0, activeObstacles.Count);
        HidingObstacle obstacle = activeObstacles[index];
        // Retirer immediatement pour que l'autre IA ne puisse pas le choisir
        activeObstacles.RemoveAt(index);
        return obstacle;
    }

    /// <summary>Lance la sequence glow + envol. Appele par HideAndSeekAI apres ClaimRandom().</summary>
    public void Remove()
    {
        if (isRemoving) return;
        isRemoving = true;
        // La liste a deja ete mise a jour par ClaimRandom() — pas de retrait ici.
        StartCoroutine(RemoveSequence());
    }

    private IEnumerator RemoveSequence()
    {
        // ── Phase 1 : Glow ────────────────────────────────────────────────
        // Activer l'emission sur tous les materiaux instances.
        Material[] mats = GetInstancedMaterials();
        foreach (Material mat in mats)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }

        float elapsed = 0f;
        while (elapsed < glowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glowDuration;
            Color emission = glowColor * (glowMaxIntensity * t);
            foreach (Material mat in mats)
                mat.SetColor("_EmissionColor", emission);
            yield return null;
        }

        // ── Phase 2 : Envol vers le haut ─────────────────────────────────
        Vector3 startPos = transform.position;
        Vector3 endPos   = startPos + Vector3.up * flyHeight;

        elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / flyDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);

            // Faire disparaitre progressivement (fade alpha si le shader le supporte)
            float alpha = 1f - t;
            foreach (Material mat in mats)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    mat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, alpha));
                }
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>Retourne des copies instanciees des materiaux pour ne pas modifier les assets partagees.</summary>
    private Material[] GetInstancedMaterials()
    {
        System.Collections.Generic.List<Material> mats = new System.Collections.Generic.List<Material>();
        foreach (Renderer r in renderers)
            mats.AddRange(r.materials); // .materials cree des instances automatiquement
        return mats.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position + Vector3.up * (flyHeight * 0.5f), new Vector3(0.2f, flyHeight, 0.2f));
    }
}
