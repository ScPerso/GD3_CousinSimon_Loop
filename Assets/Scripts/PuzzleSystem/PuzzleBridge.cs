using UnityEngine;

/// <summary>
/// Singleton persistant entre les scènes pour transmettre le résultat du puzzle à LoopHero.
/// </summary>
public class PuzzleBridge : MonoBehaviour
{
    public static PuzzleBridge Instance { get; private set; }

    public const int RewardOnSuccess = 10;
    public const int PenaltyOnFailure = -10;

    /// <summary>Indique si le puzzle a été résolu avec succès.</summary>
    public bool PuzzleSolved { get; private set; }

    /// <summary>Indique si le joueur revient du mini-jeu et qu'un résultat est en attente.</summary>
    public bool HasPendingResult { get; private set; }

    /// <summary>Index de case sur lequel le joueur se trouvait avant d'entrer dans le mini-jeu.</summary>
    public int SavedPathIndex { get; set; } = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Enregistre le résultat du puzzle avant de changer de scène.</summary>
    public void SetResult(bool solved)
    {
        PuzzleSolved = solved;
        HasPendingResult = true;
    }

    /// <summary>Consomme le résultat en attente après application dans LoopHero.</summary>
    public void ConsumeResult()
    {
        HasPendingResult = false;
    }
}
