using UnityEngine;

/// <summary>
/// Trigger invisible sous toute la scène.
/// Si le joueur le touche (raté la piscine), il est renvoyé au spawn.
/// </summary>
public class ThimbleDeathFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            ThimblePlayer.Instance?.Respawn();
    }
}
