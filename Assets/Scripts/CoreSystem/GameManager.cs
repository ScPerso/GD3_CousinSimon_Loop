using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton persistant (DontDestroyOnLoad). Remplace tous les bridges eparpilles.
/// Stocke : flags narratifs, ressources, index de case, cases visitees.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Initial Dialogue")]
    public DialogueData firstDialogue;
    public float delayBeforeFirstDialogue = 1f;

    // ─── Sauvegarde de session ───────────────────────────────────────────────

    /// <summary>True si on revient d'un mini-jeu et qu'une sauvegarde est en attente.</summary>
    public bool HasSave { get; private set; }

    /// <summary>Ressources sauvegardees avant d'entrer dans le mini-jeu.</summary>
    public int SavedResources { get; private set; }

    /// <summary>Index de case sauvegarde avant d'entrer dans le mini-jeu.</summary>
    public int SavedCaseIndex { get; private set; }

    /// <summary>Resultat du dernier mini-jeu (true = victoire, false = defaite, null = pas joue).</summary>
    public bool? MiniGameResult { get; private set; }

    /// <summary>Recompense si mini-jeu reussi.</summary>
    public int MiniGameReward { get; private set; }

    /// <summary>Penalite si mini-jeu echoue.</summary>
    public int MiniGamePenalty { get; private set; }

    // ─── Flags narratifs ─────────────────────────────────────────────────────

    private HashSet<string> narrativeFlags = new HashSet<string>();
    private bool hasShownFirstDialogue = false;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        narrativeFlags.Clear();
        HasSave = false;
    }

    private void Start()
    {
        if (firstDialogue != null && !HasFlag("game_started"))
            StartCoroutine(ShowFirstDialogue());
    }

    private IEnumerator ShowFirstDialogue()
    {
        yield return new WaitForSeconds(delayBeforeFirstDialogue);

        while (DialogueManager.Instance == null)
            yield return null;

        if (!hasShownFirstDialogue)
        {
            hasShownFirstDialogue = true;
            AddFlag("game_started");
            DialogueManager.Instance.StartDialogue(firstDialogue);
        }
    }

    // ─── API Sauvegarde ──────────────────────────────────────────────────────

    /// <summary>
    /// Sauvegarde l'etat courant juste avant de charger un mini-jeu.
    /// </summary>
    /// <param name="resources">Ressources actuelles du joueur.</param>
    /// <param name="caseIndex">Index de la case actuelle.</param>
    /// <param name="miniGameReward">Ressources gagnees si le mini-jeu est reussi.</param>
    /// <param name="miniGamePenalty">Ressources perdues si le mini-jeu est rate.</param>
    public void SaveState(int resources, int caseIndex, int miniGameReward, int miniGamePenalty)
    {
        HasSave = true;
        SavedResources = resources;
        SavedCaseIndex = caseIndex;
        MiniGameResult = null;
        MiniGameReward = miniGameReward;
        MiniGamePenalty = miniGamePenalty;

        Debug.Log($"[GameManager] Etat sauvegarde — Ressources:{resources}, Case:{caseIndex}");
    }

    /// <summary>
    /// Enregistre le resultat du mini-jeu avant de retourner a la scene principale.
    /// </summary>
    public void SetMiniGameResult(bool success)
    {
        MiniGameResult = success;
        Debug.Log($"[GameManager] Resultat mini-jeu : {(success ? "Victoire" : "Defaite")}");
    }

    /// <summary>
    /// Charge l'etat sauvegarde et le retourne. Consomme la sauvegarde (HasSave repasse a false).
    /// </summary>
    public (int resources, int caseIndex, bool? miniGameResult, int reward, int penalty) LoadState()
    {
        var data = (SavedResources, SavedCaseIndex, MiniGameResult, MiniGameReward, MiniGamePenalty);
        Debug.Log($"[GameManager] Etat charge — Ressources:{SavedResources}, Case:{SavedCaseIndex}, Resultat:{MiniGameResult}");
        return data;
    }

    /// <summary>
    /// Remet HasSave a false. Appele par GameStateManager.RestartGame().
    /// </summary>
    public void ResetSave()
    {
        HasSave = false;
        SavedResources = 0;
        SavedCaseIndex = 0;
        MiniGameResult = null;
        Debug.Log("[GameManager] Sauvegarde remise a zero.");
    }

    // ─── Flags narratifs ─────────────────────────────────────────────────────

    public void AddFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;

        bool isNew = narrativeFlags.Add(flag);
        if (isNew)
        {
            Debug.Log($"<color=green>[FLAG]</color> {flag}");

            if (flag == "trigger_game_over" && GameStateManager.Instance != null)
                GameStateManager.Instance.TriggerDefeat();
        }
    }

    public void RemoveFlag(string flag) => narrativeFlags.Remove(flag);

    public bool HasFlag(string flag) => narrativeFlags.Contains(flag);

    public int GetFlagCount() => narrativeFlags.Count;

    public void ClearAllFlags()
    {
        narrativeFlags.Clear();
        hasShownFirstDialogue = false;
        Debug.Log("[GameManager] Flags effaces.");
    }

    public void ResetGame()
    {
        ClearAllFlags();
        ResetSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
