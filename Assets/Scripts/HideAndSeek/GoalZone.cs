using UnityEngine;

/// <summary>
/// Zone d'arrivée (Point B). Quand le joueur entre dedans, déclenche la victoire.
/// </summary>
public class GoalZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GoalZone] OnTriggerEnter — collider={other.gameObject.name} tag={other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("[GoalZone] Joueur arrivé au Point B — VICTOIRE !");
            HideAndSeekManager.Instance?.TriggerVictory();
        }
    }
}
