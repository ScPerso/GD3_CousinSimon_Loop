using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnhancedQuestUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questInfoText;
    public TextMeshProUGUI currentTileInfoText;
    
    [Header("Quest Definitions")]
    public List<EnhancedQuest> quests = new List<EnhancedQuest>();
    
    private BoardTile lastActiveTile;
    
    private void Start()
    {
        UpdateQuestDisplay();
        
        if (GameManager.Instance != null)
        {
            InvokeRepeating(nameof(UpdateQuestDisplay), 1f, 1f);
        }
        
        BoardTile[] tiles = FindObjectsByType<BoardTile>(FindObjectsSortMode.None);
        foreach (BoardTile tile in tiles)
        {
            tile.OnTileActivated += HandleTileActivated;
        }
    }
    
    private void HandleTileActivated(BoardTile tile)
    {
        lastActiveTile = tile;
        UpdateCurrentTileInfo(tile);
    }
    
    public void UpdateQuestDisplay()
    {
        if (questInfoText == null || GameManager.Instance == null)
            return;
        
        string questText = "<b><size=22>OBJECTIFS</size></b>\n\n";
        
        foreach (EnhancedQuest quest in quests)
        {
            bool isComplete = quest.IsComplete();
            string statusIcon = isComplete ? "✓" : "○";
            string colorTag = isComplete ? "<color=#00FF00>" : "<color=#FFFFFF>";
            
            questText += $"{statusIcon} {colorTag}{quest.questName}</color>\n";
            questText += $"   <size=14><color=#AAAAAA>→ {GetTileColorName(quest.tileType)}</color></size>\n";
        }
        
        questText += "\n<b><size=18>CONDITION DE VICTOIRE</size></b>\n";
        questText += "<color=#FFD700>Collecter toutes les preuves</color>\n";
        questText += "<size=14><color=#AAAAAA>Trouvez tous les indices de l'enquêteur disparu</color></size>\n";
        questText += "<size=14><color=#AAAAAA>puis retournez au Capitaine avec le rapport</color></size>";
        
        questInfoText.text = questText;
    }
    
    private void UpdateCurrentTileInfo(BoardTile tile)
    {
        if (currentTileInfoText == null || tile == null || tile.TileData == null)
            return;
        
        TileData data = tile.TileData;
        string info = $"<b><size=20>CASE ACTUELLE</size></b>\n\n";
        
        info += $"<b>{data.tileName}</b>\n";
        info += $"<color=#{ColorUtility.ToHtmlStringRGB(data.tileColor)}>■</color> {GetTileTypeName(data.tileType)}\n\n";
        
        info += "<b>EFFETS :</b>\n";
        
        switch (data.tileType)
        {
            case TileType.Captain:
                info += "• Dialogue avec le Capitaine\n";
                break;
                
            case TileType.WrittenNote:
                info += "• Indice: Note écrite\n";
                info += "• <color=#FF6666>Non revisitable</color>\n";
                break;
                
            case TileType.PersonalItem:
                info += "• Indice: Objet personnel\n";
                info += "• <color=#FF6666>Non revisitable</color>\n";
                break;
                
            case TileType.Evidence:
                info += "• Indice: Preuve matérielle\n";
                info += "• <color=#FF6666>Non revisitable</color>\n";
                break;
                
            case TileType.Corpse:
                info += "• Indice: Cadavre\n";
                info += "• <color=#FF6666>Non revisitable</color>\n";
                break;
                
            case TileType.Recharge:
                info += "• Station de recharge\n";
                info += "• <color=#66FF66>+30 ressources</color>\n";
                break;
                
            case TileType.Empty:
                info += "• Aucun effet\n";
                break;
        }
        
        currentTileInfoText.text = info;
    }
    
    private string GetTileColorName(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Captain:
                return "Case <color=#3366CC>Bleue</color>";
            case TileType.WrittenNote:
                return "Case <color=#E6E6AF>Jaune pâle</color>";
            case TileType.PersonalItem:
                return "Case <color=#99CC66>Verte</color>";
            case TileType.Evidence:
                return "Case <color=#E69933>Orange</color>";
            case TileType.Corpse:
                return "Case <color=#801919>Rouge sombre</color>";
            case TileType.Recharge:
                return "Case <color=#00FFFF>Cyan</color>";
            case TileType.Empty:
                return "Case <color=#8B8B8B>Grise</color>";
            default:
                return "Case inconnue";
        }
    }
    
    private string GetTileTypeName(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Captain: return "Capitaine";
            case TileType.WrittenNote: return "Note";
            case TileType.PersonalItem: return "Objet";
            case TileType.Evidence: return "Preuve";
            case TileType.Corpse: return "Cadavre";
            case TileType.Recharge: return "Recharge";
            case TileType.Empty: return "Vide";
            default: return "Inconnu";
        }
    }
    
    private void OnDestroy()
    {
        BoardTile[] tiles = FindObjectsByType<BoardTile>(FindObjectsSortMode.None);
        foreach (BoardTile tile in tiles)
        {
            tile.OnTileActivated -= HandleTileActivated;
        }
    }
}

[System.Serializable]
public class EnhancedQuest
{
    public string questName;
    public string requiredFlag;
    public TileType tileType;
    
    public bool IsComplete()
    {
        if (GameManager.Instance == null || string.IsNullOrEmpty(requiredFlag))
            return false;
        
        return GameManager.Instance.HasFlag(requiredFlag);
    }
}
