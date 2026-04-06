using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Serializable]
public class TileActionEvent : UnityEvent<GameObject, BoardTile> { }

public class TileActionHandler : MonoBehaviour
{
    [Header("Tile Type Events")]
    public TileActionEvent onCaptainTile;
    public TileActionEvent onWrittenNoteTile;
    public TileActionEvent onPersonalItemTile;
    public TileActionEvent onEvidenceTile;
    public TileActionEvent onCorpseTile;
    public TileActionEvent onEmptyTile;
    public TileActionEvent onPuzzleTile;
    public TileActionEvent onHideAndSeekTile;

    [Header("Puzzle Tile")]
    public string puzzleSceneName = "PuzzleScene";

    [Header("HideAndSeek Tile")]
    public string hideAndSeekSceneName = "Mini-Jeu1";

    [Header("Captain Tile")]
    public DialogueData captainFirstDialogue;
    public DialogueData captainReturningDialogue;
    public DialogueData captainFinalDialogue;

    [Header("Clue Tiles - First Visit")]
    public DialogueData writtenNoteDialogue;
    public DialogueData personalItemDialogue;
    public DialogueData evidenceDialogue;
    public DialogueData corpseDialogue;

    [Header("Clue Tiles - Revisit")]
    public DialogueData writtenNoteRevisitDialogue;
    public DialogueData personalItemRevisitDialogue;
    public DialogueData evidenceRevisitDialogue;
    public DialogueData corpseRevisitDialogue;

    private void OnEnable()
    {
        BoardManager.OnBoardGenerated += RebindTileEvents;
    }

    private void OnDisable()
    {
        BoardManager.OnBoardGenerated -= RebindTileEvents;
    }

    private void Start()
    {
        // Résultats en différé pour laisser le temps aux singletons de s'initialiser
        StartCoroutine(ApplyPendingResultsDelayed());
    }

    /// <summary>Rebind les events sur toutes les tuiles fraîchement générées.</summary>
    private void RebindTileEvents()
    {
        BoardTile[] tiles = FindObjectsByType<BoardTile>(FindObjectsSortMode.None);
        foreach (BoardTile tile in tiles)
        {
            tile.OnTileActivated -= HandleTileActivation;
            tile.OnTileActivated += HandleTileActivation;
        }
        Debug.Log($"[TileActionHandler] Events rebindés sur {tiles.Length} tuiles.");
    }

    private System.Collections.IEnumerator ApplyPendingResultsDelayed()
    {
        // Attendre que ResourceManager soit prêt
        float timeout = 3f;
        float elapsed = 0f;
        while (ResourceManager.Instance == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyPuzzleResultIfPending();
        ApplyHideAndSeekResultIfPending();
    }

    /// <summary>Applique le résultat du mini-jeu cache-cache si on vient d'en revenir.</summary>
    private void ApplyHideAndSeekResultIfPending()
    {
        if (!HideAndSeekBridge.HasPendingResult) return;

        bool success = HideAndSeekBridge.ConsumeResult();
        if (success)
        {
            ResourceManager.Instance?.AddResources(HideAndSeekBridge.RewardOnSuccess);
            Debug.Log($"[HideAndSeek] Mini-jeu réussi ! +{HideAndSeekBridge.RewardOnSuccess} ressources.");
        }
        else
        {
            int penalty = Mathf.Abs(HideAndSeekBridge.PenaltyOnFailure);
            ResourceManager.Instance?.RemoveResources(penalty);
            Debug.Log($"[HideAndSeek] Mini-jeu échoué. -{penalty} ressources.");
        }
    }

    /// <summary>Applique le résultat du mini-jeu puzzle si on vient d'en revenir.</summary>
    private void ApplyPuzzleResultIfPending()
    {
        if (PuzzleBridge.Instance == null || !PuzzleBridge.Instance.HasPendingResult)
            return;

        if (PuzzleBridge.Instance.PuzzleSolved)
        {
            ResourceManager.Instance?.AddResources(PuzzleBridge.RewardOnSuccess);
            Debug.Log($"[Puzzle] Puzzle réussi ! +{PuzzleBridge.RewardOnSuccess} ressources.");
        }
        else
        {
            int penalty = Mathf.Abs(PuzzleBridge.PenaltyOnFailure);
            ResourceManager.Instance?.RemoveResources(penalty);
            Debug.Log($"[Puzzle] Puzzle échoué. -{penalty} ressources.");
        }

        PuzzleBridge.Instance.ConsumeResult();
    }

    private void HandleTileActivation(BoardTile tile)
    {
        GameObject activator = GameObject.FindGameObjectWithTag("Player");

        switch (tile.tileData.tileType)
        {
            case TileType.Captain:
                ExecuteCaptainAction(activator, tile);
                break;
            case TileType.WrittenNote:
                ExecuteWrittenNoteAction(activator, tile);
                break;
            case TileType.PersonalItem:
                ExecutePersonalItemAction(activator, tile);
                break;
            case TileType.Evidence:
                ExecuteEvidenceAction(activator, tile);
                break;
            case TileType.Corpse:
                ExecuteCorpseAction(activator, tile);
                break;
            case TileType.Recharge:
                ExecuteRechargeAction(activator, tile);
                break;
            case TileType.Puzzle:
                ExecutePuzzleAction(activator, tile);
                break;
            case TileType.HideAndSeek:
                ExecuteHideAndSeekAction(activator, tile);
                break;
            case TileType.Empty:
                ExecuteEmptyAction(activator, tile);
                break;
        }
    }

    private void ExecuteCaptainAction(GameObject activator, BoardTile tile)
    {
        Debug.Log("Captain Tile: Rencontre avec le Capitaine");

        if (TileNameDisplay.Instance != null)
        {
            TileNameDisplay.Instance.ShowTileName("Bureau du Capitaine");
        }

        DialogueData dialogueToUse = GetAppropriateCaptainDialogue();

        if (DialogueManager.Instance != null && dialogueToUse != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToUse);
        }

        onCaptainTile?.Invoke(activator, tile);
    }

    private DialogueData GetAppropriateCaptainDialogue()
    {
        if (GameManager.Instance == null)
            return captainFirstDialogue;

        bool hasAllClues = GameManager.Instance.HasFlag("clue_note_collected") 
                          && GameManager.Instance.HasFlag("clue_item_collected") 
                          && GameManager.Instance.HasFlag("clue_evidence_collected")
                          && GameManager.Instance.HasFlag("clue_corpse_found");
        
        bool hasMetCaptain = GameManager.Instance.HasFlag("met_captain");

        if (hasAllClues && captainFinalDialogue != null)
        {
            Debug.Log("Captain dialogue: FINAL (toutes les preuves trouvées)");
            return captainFinalDialogue;
        }
        else if (hasMetCaptain && captainReturningDialogue != null)
        {
            Debug.Log("Captain dialogue: RETURNING");
            return captainReturningDialogue;
        }
        else
        {
            Debug.Log("Captain dialogue: FIRST");
            return captainFirstDialogue;
        }
    }

    private void ExecuteWrittenNoteAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
        {
            TileNameDisplay.Instance.ShowTileName("Note découverte");
        }

        Debug.Log("WrittenNote Tile: Une note laissée par l'enquêteur");

        string tileVisitFlag = $"visited_note_{tile.gridPosition.x}_{tile.gridPosition.y}";
        bool isFirstVisit = GameManager.Instance != null && !GameManager.Instance.HasFlag(tileVisitFlag);
        
        if (GameManager.Instance != null && isFirstVisit)
        {
            GameManager.Instance.AddFlag(tileVisitFlag);
            GameManager.Instance.AddFlag("clue_note_collected");
            Debug.Log($"Flag set: {tileVisitFlag} and clue_note_collected");
            CheckForAllCluesCollected();
        }

        if (DialogueManager.Instance != null)
        {
            if (isFirstVisit && writtenNoteDialogue != null)
            {
                Debug.Log("Starting WrittenNote dialogue (first visit)");
                DialogueManager.Instance.StartDialogue(writtenNoteDialogue);
            }
            else if (!isFirstVisit && writtenNoteRevisitDialogue != null)
            {
                Debug.Log("Starting WrittenNote revisit dialogue");
                DialogueManager.Instance.StartDialogue(writtenNoteRevisitDialogue);
            }
        }

        tile.MarkAsVisited();
        onWrittenNoteTile?.Invoke(activator, tile);
    }

    private void ExecutePersonalItemAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
        {
            TileNameDisplay.Instance.ShowTileName("Objet personnel");
        }

        Debug.Log("PersonalItem Tile: Un objet de l'enquêteur");

        string tileVisitFlag = $"visited_item_{tile.gridPosition.x}_{tile.gridPosition.y}";
        bool isFirstVisit = GameManager.Instance != null && !GameManager.Instance.HasFlag(tileVisitFlag);
        
        if (GameManager.Instance != null && isFirstVisit)
        {
            GameManager.Instance.AddFlag(tileVisitFlag);
            GameManager.Instance.AddFlag("clue_item_collected");
            Debug.Log($"Flag set: {tileVisitFlag} and clue_item_collected");
            CheckForAllCluesCollected();
        }

        if (DialogueManager.Instance != null)
        {
            if (isFirstVisit && personalItemDialogue != null)
            {
                Debug.Log("Starting PersonalItem dialogue (first visit)");
                DialogueManager.Instance.StartDialogue(personalItemDialogue);
            }
            else if (!isFirstVisit && personalItemRevisitDialogue != null)
            {
                Debug.Log("Starting PersonalItem revisit dialogue");
                DialogueManager.Instance.StartDialogue(personalItemRevisitDialogue);
            }
        }

        tile.MarkAsVisited();
        onPersonalItemTile?.Invoke(activator, tile);
    }

    private void ExecuteEvidenceAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
        {
            TileNameDisplay.Instance.ShowTileName("Preuve découverte");
        }

        Debug.Log("Evidence Tile: Des traces suspectes");

        string tileVisitFlag = $"visited_evidence_{tile.gridPosition.x}_{tile.gridPosition.y}";
        bool isFirstVisit = GameManager.Instance != null && !GameManager.Instance.HasFlag(tileVisitFlag);
        
        if (GameManager.Instance != null && isFirstVisit)
        {
            GameManager.Instance.AddFlag(tileVisitFlag);
            GameManager.Instance.AddFlag("clue_evidence_collected");
            Debug.Log($"Flag set: {tileVisitFlag} and clue_evidence_collected");
            CheckForAllCluesCollected();
        }

        if (DialogueManager.Instance != null)
        {
            if (isFirstVisit && evidenceDialogue != null)
            {
                Debug.Log("Starting Evidence dialogue (first visit)");
                DialogueManager.Instance.StartDialogue(evidenceDialogue);
            }
            else if (!isFirstVisit && evidenceRevisitDialogue != null)
            {
                Debug.Log("Starting Evidence revisit dialogue (GAME OVER after dialogue)");
                DialogueManager.Instance.StartDialogue(evidenceRevisitDialogue);
            }
        }

        tile.MarkAsVisited();
        onEvidenceTile?.Invoke(activator, tile);
    }

    private void ExecuteCorpseAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
        {
            TileNameDisplay.Instance.ShowTileName("Scène du crime");
        }

        Debug.Log("Corpse Tile: Le cadavre de l'enquêteur");

        string tileVisitFlag = $"visited_corpse_{tile.gridPosition.x}_{tile.gridPosition.y}";
        bool isFirstVisit = GameManager.Instance != null && !GameManager.Instance.HasFlag(tileVisitFlag);
        
        if (GameManager.Instance != null && isFirstVisit)
        {
            GameManager.Instance.AddFlag(tileVisitFlag);
            GameManager.Instance.AddFlag("clue_corpse_found");
            Debug.Log($"Flag set: {tileVisitFlag} and clue_corpse_found");
            CheckForAllCluesCollected();
        }

        if (DialogueManager.Instance != null)
        {
            if (isFirstVisit && corpseDialogue != null)
            {
                Debug.Log("Starting Corpse dialogue (first visit)");
                DialogueManager.Instance.StartDialogue(corpseDialogue);
            }
            else if (!isFirstVisit && corpseRevisitDialogue != null)
            {
                Debug.Log("Starting Corpse revisit dialogue");
                DialogueManager.Instance.StartDialogue(corpseRevisitDialogue);
            }
        }

        tile.MarkAsVisited();
        onCorpseTile?.Invoke(activator, tile);
    }

    private void ExecuteRechargeAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
        {
            TileNameDisplay.Instance.ShowTileName("Station de recharge");
        }

        Debug.Log("Recharge Tile: Station de recharge activée");

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddResources(30);
            Debug.Log("+30 ressources récupérées!");
        }

        tile.MarkAsVisited();
    }

    private void ExecutePuzzleAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
            TileNameDisplay.Instance.ShowTileName("Puzzle mystérieux");

        Debug.Log("[Puzzle] Case puzzle activée, chargement du mini-jeu...");

        tile.MarkAsVisited();
        onPuzzleTile?.Invoke(activator, tile);

        // Sauvegarder la position courante pour y revenir après le mini-jeu
        if (PuzzleBridge.Instance != null && PlayerLoopController.Instance != null)
            PuzzleBridge.Instance.SavedPathIndex = PlayerLoopController.Instance.CurrentPathIndex;

        if (PlayerLoopController.Instance != null)
            PlayerLoopController.Instance.ForceEndTurnForSceneChange();

        SceneManager.LoadScene(puzzleSceneName);
    }

    private void CheckForAllCluesCollected()
    {
        if (GameManager.Instance == null || DialogueManager.Instance == null)
            return;

        bool hasNote = GameManager.Instance.HasFlag("clue_note_collected");
        bool hasItem = GameManager.Instance.HasFlag("clue_item_collected");
        bool hasEvidence = GameManager.Instance.HasFlag("clue_evidence_collected");
        bool hasCorpse = GameManager.Instance.HasFlag("clue_corpse_found");

        if (hasNote && hasItem && hasEvidence && hasCorpse)
        {
            Debug.Log("Toutes les preuves ont été trouvées! Le Capitaine attend votre rapport.");
            GameManager.Instance.AddFlag("all_clues_collected");
        }
    }

    private void ExecuteHideAndSeekAction(GameObject activator, BoardTile tile)
    {
        if (TileNameDisplay.Instance != null)
            TileNameDisplay.Instance.ShowTileName("Cache-cache !");

        Debug.Log("[HideAndSeek] Case cache-cache activée, chargement du mini-jeu...");

        tile.MarkAsVisited();
        onHideAndSeekTile?.Invoke(activator, tile);

        // Sauvegarder la position courante pour y revenir après le mini-jeu
        if (PlayerLoopController.Instance != null)
            HideAndSeekBridge.SavedPathIndex = PlayerLoopController.Instance.CurrentPathIndex;

        if (PlayerLoopController.Instance != null)
            PlayerLoopController.Instance.ForceEndTurnForSceneChange();

        SceneManager.LoadScene(hideAndSeekSceneName);
    }

    private void ExecuteEmptyAction(GameObject activator, BoardTile tile)
    {
        Debug.Log("Empty Tile: Nothing happens");
        onEmptyTile?.Invoke(activator, tile);
    }

    private void OnDestroy()
    {
        BoardManager.OnBoardGenerated -= RebindTileEvents;

        BoardTile[] tiles = FindObjectsByType<BoardTile>(FindObjectsSortMode.None);
        foreach (BoardTile tile in tiles)
        {
            tile.OnTileActivated -= HandleTileActivation;
        }
    }
}
