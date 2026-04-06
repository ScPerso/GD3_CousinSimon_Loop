using UnityEngine;

/// <summary>
/// Singleton statique (pas de MonoBehaviour) qui transporte le résultat du mini-jeu
/// entre les scènes, sur le même modèle que PuzzleBridge.
/// </summary>
public static class HideAndSeekBridge
{
    public const int RewardOnSuccess  =  10;
    public const int PenaltyOnFailure = -10;

    /// <summary>true = victoire, false = défaite. null = pas encore joué.</summary>
    public static bool? PendingResult { get; set; }

    /// <summary>Index de case sur lequel le joueur se trouvait avant d'entrer dans le mini-jeu.</summary>
    public static int SavedPathIndex { get; set; } = -1;

    public static bool HasPendingResult => PendingResult.HasValue;

    /// <summary>Consomme et remet à null le résultat en attente.</summary>
    public static bool ConsumeResult()
    {
        bool result = PendingResult ?? false;
        PendingResult = null;
        return result;
    }
}
