using UnityEngine;

/// <summary>
/// Cube de la grille piscine du mini-jeu Dé à coudre.
///
/// États :
///   - Invisible (défaut) : le joueur peut sauter dessus sans conséquence visible.
///   - Révélé (rouge)     : premier contact passé. Si le joueur atterrit dessus → DEFAITE.
///
/// Ce cube n'a pas de collision physique (isTrigger = true sur le Collider).
/// Il est invisible en jeu par défaut (MeshRenderer désactivé).
/// Quand le joueur entre dans le trigger :
///   - Si invisible → devient rouge + respawn joueur + signale un cube révélé au ThimbleManager.
///   - Si déjà rouge → déclenche la défaite.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class ThimbleWaterCube : MonoBehaviour
{
    private static readonly Color RevealedColor = Color.red;

    private bool isRevealed;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider  = GetComponent<BoxCollider>();

        // Trigger sans collision physique
        boxCollider.isTrigger = true;

        // Invisible par défaut
        meshRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (ThimbleManager.Instance == null || !ThimbleManager.Instance.IsPlaying) return;

        if (isRevealed)
        {
            // Cube déjà rouge → défaite
            Debug.Log($"[ThimbleWaterCube] Cube rouge touché : {name} → DEFAITE.");
            ThimbleManager.Instance.TriggerDefeat();
        }
        else
        {
            // Premier contact → révéler le cube, respawn, incrémenter le compteur
            Reveal();
            ThimblePlayer.Instance?.Respawn();
            ThimbleManager.Instance.OnCubeRevealed();
        }
    }

    /// <summary>Rend le cube visible en rouge.</summary>
    private void Reveal()
    {
        isRevealed = true;
        meshRenderer.enabled = true;
        meshRenderer.material.color = RevealedColor;
        Debug.Log($"[ThimbleWaterCube] Cube révélé : {name}.");
    }
}
