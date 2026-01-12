using UnityEngine;
using UnityEditor;

public class CreateInternalDialogues
{
    [MenuItem("Tools/Create Internal Dialogues")]
    public static void CreateDialogues()
    {
        string basePath = "Assets/ScriptableObjects/Dialogues";

        UpdateNoteFirstDialogue(basePath);
        CreateCorpseFirstDialogue(basePath);
        CreateCorpseRevisitDialogue(basePath);
        CreateNoteRevisitDialogue(basePath);
        CreateEvidenceFirstDialogue(basePath);
        CreateEvidenceRevisitDialogue(basePath);
        CreatePersonalItemFirstDialogue(basePath);
        CreatePersonalItemRevisitDialogue(basePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        AssignDialoguesToTileActionHandler(basePath);
        
        Debug.Log("All internal dialogues created and assigned successfully!");
    }

    private static void AssignDialoguesToTileActionHandler(string basePath)
    {
        TileActionHandler tileActionHandler = Object.FindObjectOfType<TileActionHandler>();
        
        if (tileActionHandler == null)
        {
            Debug.LogWarning("TileActionHandler not found in scene!");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(tileActionHandler);

        AssignDialogue(serializedObject, "writtenNoteRevisitDialogue", $"{basePath}/NoteRevisitDialogue.asset");
        AssignDialogue(serializedObject, "corpseRevisitDialogue", $"{basePath}/CorpseRevisitDialogue.asset");
        AssignDialogue(serializedObject, "evidenceRevisitDialogue", $"{basePath}/EvidenceRevisitDialogue.asset");
        AssignDialogue(serializedObject, "personalItemRevisitDialogue", $"{basePath}/PersonalItemRevisitDialogue.asset");

        serializedObject.ApplyModifiedProperties();
        
        Debug.Log("Dialogue assets assigned to TileActionHandler");
    }

    private static void AssignDialogue(SerializedObject serializedObject, string propertyName, string assetPath)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
            if (dialogue != null)
            {
                property.objectReferenceValue = dialogue;
                Debug.Log($"Assigned {assetPath} to {propertyName}");
            }
        }
    }

    private static void CreateCorpseFirstDialogue(string basePath)
    {
        string existingPath = $"{basePath}/CorpseDialogue.asset";
        DialogueData existingDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(existingPath);
        
        if (existingDialogue != null)
        {
            DialogueNode node = existingDialogue.startNode;
            if (node != null)
            {
                node.speakerName = "Vous";
                node.dialogueText = "Alors il est déjà mort... Son visage est à peine reconnaissable.";
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(existingDialogue);
                Debug.Log("Updated CorpseDialogue with new internal dialogue text");
            }
        }
    }

    private static void CreateCorpseRevisitDialogue(string basePath)
    {
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueName = "Corpse_Revisit";
        
        DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
        node.speakerName = "Vous";
        node.dialogueText = "Cet endroit est un vrai labyrinthe, et ce corps est encore plus amoché...";
        
        dialogue.startNode = node;
        
        AssetDatabase.CreateAsset(dialogue, $"{basePath}/CorpseRevisitDialogue.asset");
        AssetDatabase.AddObjectToAsset(node, dialogue);
    }

    private static void CreateNoteRevisitDialogue(string basePath)
    {
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueName = "Note_Revisit";
        
        DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
        node.speakerName = "Vous";
        node.dialogueText = "Je tourne en rond...";
        
        dialogue.startNode = node;
        
        AssetDatabase.CreateAsset(dialogue, $"{basePath}/NoteRevisitDialogue.asset");
        AssetDatabase.AddObjectToAsset(node, dialogue);
    }

    private static void UpdateNoteFirstDialogue(string basePath)
    {
        string existingPath = $"{basePath}/WrittenNoteDialogue.asset";
        DialogueData existingDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(existingPath);
        
        if (existingDialogue != null)
        {
            DialogueNode node = existingDialogue.startNode;
            if (node != null)
            {
                node.speakerName = "Vous";
                node.dialogueText = "L'enquêteur a laissé une note ? \"Le temps presse... Si vous lisez ceci, ne restez pas trop longtemps dans les parages, quelque chose ou quelqu'un ne veut pas que l'on fouine\".";
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(existingDialogue);
                Debug.Log("Updated WrittenNoteDialogue with new internal dialogue text");
            }
        }
    }

    private static void CreateEvidenceFirstDialogue(string basePath)
    {
        string existingPath = $"{basePath}/EvidenceDialogue.asset";
        DialogueData existingDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(existingPath);
        
        if (existingDialogue != null)
        {
            DialogueNode node = existingDialogue.startNode;
            if (node != null)
            {
                node.speakerName = "Vous";
                node.dialogueText = "Qu'est-ce que cette chose que j'aperçois ?! Un... monstre ? Je ferais mieux de ne pas passer par ici à nouveau...";
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(existingDialogue);
                Debug.Log("Updated EvidenceDialogue with new internal dialogue text");
            }
        }
    }

    private static void CreateEvidenceRevisitDialogue(string basePath)
    {
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueName = "Evidence_Revisit";
        
        DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
        node.speakerName = "Vous";
        node.dialogueText = "Oh non... Cette chose m'a vu !";
        
        dialogue.startNode = node;
        
        AssetDatabase.CreateAsset(dialogue, $"{basePath}/EvidenceRevisitDialogue.asset");
        AssetDatabase.AddObjectToAsset(node, dialogue);
    }

    private static void CreatePersonalItemFirstDialogue(string basePath)
    {
        string existingPath = $"{basePath}/PersonalItemDialogue.asset";
        DialogueData existingDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(existingPath);
        
        if (existingDialogue != null)
        {
            DialogueNode node = existingDialogue.startNode;
            if (node != null)
            {
                node.speakerName = "Vous";
                node.dialogueText = "Le précédent enquêteur a laissé tomber sa loupe ? Vu les traces, il a dû tomber. Il était visiblement très effrayé pour oublier ses affaires.";
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(existingDialogue);
                Debug.Log("Updated PersonalItemDialogue with new internal dialogue text");
            }
        }
    }

    private static void CreatePersonalItemRevisitDialogue(string basePath)
    {
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueName = "PersonalItem_Revisit";
        
        DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
        node.speakerName = "Vous";
        node.dialogueText = "Je suis déjà passé par là...";
        
        dialogue.startNode = node;
        
        AssetDatabase.CreateAsset(dialogue, $"{basePath}/PersonalItemRevisitDialogue.asset");
        AssetDatabase.AddObjectToAsset(node, dialogue);
    }
}
