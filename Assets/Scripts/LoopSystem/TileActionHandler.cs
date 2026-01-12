using System;
using UnityEngine;
using UnityEngine.Events;

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

    private void Start()
    {
        BoardTile[] tiles = FindObjectsByType<BoardTile>(FindObjectsSortMode.None);
        foreach (BoardTile tile in tiles)
        {
            tile.OnTileActivated += HandleTileActivation;
        }
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

    private void ExecuteEmptyAction(GameObject activator, BoardTile tile)
    {
        Debug.Log("Empty Tile: Nothing happens");
        onEmptyTile?.Invoke(activator, tile);
    }

    private void OnDestroy()
    {
        BoardTile[] tiles = FindObjectsByType<BoardTile>(FindObjectsSortMode.None);
        foreach (BoardTile tile in tiles)
        {
            tile.OnTileActivated -= HandleTileActivation;
        }
    }
}
