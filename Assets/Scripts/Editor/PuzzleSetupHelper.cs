using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Outil éditeur pour configurer automatiquement la scène PuzzleScene :
/// timer 20s, slots, pièces, résultats, TileData.
/// </summary>
public class PuzzleSetupHelper
{
    [MenuItem("Tools/HideAndSeek/Create HideAndSeekTile + Add to BoardManager")]
    public static void CreateHideAndSeekTileAndAddToBoard()
    {
        const string assetPath = "Assets/ScriptableObjects/Tiles/HideAndSeekTile.asset";
        const string mainScene = "Assets/Scenes/SampleScene.unity";

        // 1. Créer ou récupérer l'asset
        TileData existing = AssetDatabase.LoadAssetAtPath<TileData>(assetPath);
        if (existing == null)
        {
            TileData tile = ScriptableObject.CreateInstance<TileData>();
            tile.tileType      = TileType.HideAndSeek;
            tile.tileName      = "Cache-cache !";
            tile.description   = "Une ombre vous observe. Foncez vers la sortie sans vous faire attraper.";
            tile.tileColor     = new Color(1f, 0.5f, 0.05f, 1f);  // Orange vif
            tile.isPassable    = true;
            tile.triggersOnEnter = true;
            tile.canBeRevisited = true;

            AssetDatabase.CreateAsset(tile, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            existing = AssetDatabase.LoadAssetAtPath<TileData>(assetPath);
            Debug.Log("[HideAndSeekSetup] HideAndSeekTile.asset créé.");
        }
        else
        {
            // Forcer la couleur orange même si l'asset existait déjà avec une autre couleur
            existing.tileType        = TileType.HideAndSeek;
            existing.tileName        = "Cache-cache !";
            existing.description     = "Une ombre vous observe. Foncez vers la sortie sans vous faire attraper.";
            existing.tileColor       = new Color(1f, 0.5f, 0.05f, 1f);  // Orange vif
            existing.isPassable      = true;
            existing.triggersOnEnter = true;
            existing.canBeRevisited  = true;
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            Debug.Log("[HideAndSeekSetup] HideAndSeekTile.asset mis à jour (couleur orange).");
        }

        // 2. Ouvrir SampleScene et ajouter l'asset au BoardManager
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(mainScene);
        bool wasLoaded = scene.isLoaded;
        if (!wasLoaded)
            scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScene, UnityEditor.SceneManagement.OpenSceneMode.Single);

        BoardManager boardManager = Object.FindFirstObjectByType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("[HideAndSeekSetup] BoardManager introuvable dans SampleScene !");
            return;
        }

        // Vérifier si déjà présent
        System.Collections.Generic.List<TileData> types =
            new System.Collections.Generic.List<TileData>(boardManager.availableTileTypes ?? new TileData[0]);

        if (!types.Contains(existing))
        {
            types.Add(existing);
            boardManager.availableTileTypes = types.ToArray();
            UnityEditor.EditorUtility.SetDirty(boardManager);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log($"[HideAndSeekSetup] HideAndSeekTile ajouté au BoardManager ({types.Count} types au total).");
        }
        else
        {
            Debug.Log("[HideAndSeekSetup] HideAndSeekTile déjà présent dans BoardManager.");
        }

        Selection.activeObject = existing;
    }
    [MenuItem("Tools/Puzzle/1 - Setup PuzzleScene")]
    public static void SetupPuzzleScene()
    {
        string scenePath = "Assets/Scenes/PuzzleScene.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
        if (!scene.isLoaded)
            scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);

        // PuzzleBridge
        GameObject bridgeGo = GameObject.Find("PuzzleBridge");
        if (bridgeGo != null && bridgeGo.GetComponent<PuzzleBridge>() == null)
            bridgeGo.AddComponent<PuzzleBridge>();

        // PuzzleSceneManager
        GameObject managerGo = GameObject.Find("PuzzleSceneManager");
        PuzzleSceneManager manager = managerGo?.GetComponent<PuzzleSceneManager>();
        if (managerGo != null && manager == null)
            manager = managerGo.AddComponent<PuzzleSceneManager>();

        if (manager == null)
        {
            Debug.LogError("[PuzzleSetupHelper] PuzzleSceneManager GameObject introuvable !");
            return;
        }

        // Slots
        manager.slots = AttachSlotsToBoard();

        // PieceTray
        GameObject trayGo = GameObject.Find("PieceTray");
        if (trayGo != null) manager.pieceTray = trayGo.transform;

        // ResultPanel
        GameObject resultPanelGo = GameObject.Find("ResultPanel");
        if (resultPanelGo != null)
        {
            manager.resultPanel = resultPanelGo;
            manager.resultPanelBackground = resultPanelGo.GetComponent<Image>();
            resultPanelGo.SetActive(false);

            // RectTransform du panel
            RectTransform resultRect = resultPanelGo.GetComponent<RectTransform>();
            if (resultRect != null)
            {
                resultRect.sizeDelta = new Vector2(650, 340);
                resultRect.anchoredPosition = Vector2.zero;
            }

            // ResultText — position haute dans le panel
            Transform resultTextTr = resultPanelGo.transform.Find("ResultText");
            if (resultTextTr != null)
            {
                TextMeshProUGUI tmp = EnsureTMP(resultTextTr.gameObject);
                tmp.text = "Puzzle résolu !";
                tmp.fontSize = 72;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(0.2f, 0.9f, 0.2f);
                RectTransform rt = resultTextTr.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(600, 100);
                rt.anchoredPosition = new Vector2(0, 60);
                manager.resultText = tmp;
            }

            // ResourceEffectText — position basse dans le panel
            Transform resourceTextTr = resultPanelGo.transform.Find("ResourceEffectText");
            if (resourceTextTr != null)
            {
                TextMeshProUGUI tmp = EnsureTMP(resourceTextTr.gameObject);
                tmp.text = "+10 Ressources";
                tmp.fontSize = 52;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(0.2f, 0.9f, 0.2f);
                RectTransform rt = resourceTextTr.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(600, 80);
                rt.anchoredPosition = new Vector2(0, -40);
                manager.resourceEffectText = tmp;
            }
        }

        // --- Timer UI (créé s'il n'existe pas) ---
        GameObject timerGo = CreateOrFindTimerUI();
        if (timerGo != null)
        {
            TextMeshProUGUI timerTmp = EnsureTMP(timerGo);
            timerTmp.text = "20";
            timerTmp.fontSize = 80;
            timerTmp.fontStyle = FontStyles.Bold;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerTmp.color = Color.white;

            RectTransform timerRect = timerGo.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.sizeDelta = new Vector2(120, 100);
            timerRect.anchoredPosition = new Vector2(0, -10);

            manager.timerText = timerTmp;
        }

        // ReturnButton — supprimé, plus besoin

        // Titre
        GameObject titleGo = GameObject.Find("Title");
        if (titleGo != null)
        {
            TextMeshProUGUI tmp = EnsureTMP(titleGo);
            tmp.text = "PUZZLE MYSTÉRIEUX";
            tmp.fontSize = 48;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.9f, 0.8f, 0.4f);

            RectTransform titleRect = titleGo.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                titleRect.sizeDelta = new Vector2(800, 70);
                titleRect.anchoredPosition = new Vector2(0, -55);
            }
        }

        // PuzzleBoard size
        RectTransform boardRect = GameObject.Find("PuzzleBoard")?.GetComponent<RectTransform>();
        if (boardRect != null)
        {
            boardRect.sizeDelta = new Vector2(530, 530);
            boardRect.anchoredPosition = new Vector2(0, 60);
        }

        // PieceTray size
        RectTransform trayRect = trayGo?.GetComponent<RectTransform>();
        if (trayRect != null)
            trayRect.sizeDelta = new Vector2(0, 115);

        // Sprites & prefab (recréer le prefab proprement à chaque fois)
        manager.puzzleSprites = LoadPuzzleSprites();
        RecreatePiecePrefab();
        manager.piecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PuzzlePieceClean.prefab");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[PuzzleSetupHelper] PuzzleScene configurée avec succès !");
    }

    [MenuItem("Tools/Puzzle/3 - Recreate PuzzlePiece Prefab")]
    public static void RecreatePiecePrefab()
    {
        string prefabPath = "Assets/Prefabs/PuzzlePieceClean.prefab";

        // Supprimer l'ancien asset pour repartir propre
        if (AssetDatabase.LoadAssetAtPath<Object>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
            AssetDatabase.Refresh();
        }

        // Nouvelle scène vide pour éviter que Unity ajoute un Canvas parent
        var tempScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Additive);

        GameObject pieceGo = new GameObject("PuzzlePieceClean");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(pieceGo, tempScene);

        RectTransform rt = pieceGo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(90f, 90f);

        Image img = pieceGo.AddComponent<Image>();
        img.preserveAspect = false;
        img.raycastTarget = true;
        img.color = Color.white;

        pieceGo.AddComponent<CanvasGroup>();
        pieceGo.AddComponent<PuzzlePiece>();

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(pieceGo, prefabPath);

        // Fermer la scène temporaire sans sauvegarder
        UnityEditor.SceneManagement.EditorSceneManager.CloseScene(tempScene, true);

        AssetDatabase.Refresh();
        Debug.Log("[PuzzleSetupHelper] PuzzlePieceClean.prefab recréé sans Canvas parasite.");
        Selection.activeObject = savedPrefab;
    }

    [MenuItem("Tools/Puzzle/2 - Create PuzzleTile Asset")]
    public static void CreatePuzzleTileAsset()
    {
        string path = "Assets/ScriptableObjects/Tiles/PuzzleTile.asset";
        if (AssetDatabase.LoadAssetAtPath<TileData>(path) != null)
        {
            Debug.Log("[PuzzleSetupHelper] PuzzleTile.asset existe déjà.");
            return;
        }

        TileData asset = ScriptableObject.CreateInstance<TileData>();
        asset.tileName = "Puzzle mystérieux";
        asset.tileType = TileType.Puzzle;
        asset.description = "Un puzzle étrange qui demande votre attention. Résolvez-le pour gagner des ressources.";
        asset.tileColor = new Color(0.6f, 0.2f, 0.8f, 1f);
        asset.isPassable = true;
        asset.triggersOnEnter = true;
        asset.canBeRevisited = true;

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        Debug.Log($"[PuzzleSetupHelper] PuzzleTile.asset créé à {path}");
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static PuzzleSlot[] AttachSlotsToBoard()
    {
        PuzzleSlot[] slots = new PuzzleSlot[9];
        for (int i = 1; i <= 9; i++)
        {
            GameObject slotGo = GameObject.Find($"Slot_{i}");
            if (slotGo != null)
            {
                PuzzleSlot slot = slotGo.GetComponent<PuzzleSlot>() ?? slotGo.AddComponent<PuzzleSlot>();
                slot.correctPieceIndex = i;
                slots[i - 1] = slot;
            }
        }
        return slots;
    }

    private static GameObject CreateOrFindTimerUI()
    {
        GameObject existing = GameObject.Find("TimerText");
        if (existing != null) return existing;

        // Créer sous le Canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return null;

        GameObject timerGo = new GameObject("TimerText");
        timerGo.transform.SetParent(canvas.transform, false);
        timerGo.AddComponent<RectTransform>();
        return timerGo;
    }

    private static TextMeshProUGUI EnsureTMP(GameObject go)
    {
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        return tmp != null ? tmp : go.AddComponent<TextMeshProUGUI>();
    }

    private static Sprite[] LoadPuzzleSprites()
    {
        Sprite[] sprites = new Sprite[9];
        for (int i = 1; i <= 9; i++)
        {
            string path = $"Assets/Sprite/Puzzle/{i}.png";
            sprites[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprites[i - 1] == null)
                Debug.LogWarning($"[PuzzleSetupHelper] Sprite non trouvé : {path}");
        }
        return sprites;
    }

    private static GameObject CreatePiecePrefab()
    {
        string prefabPath = "Assets/Prefabs/PuzzlePiece.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject pieceGo = new GameObject("PuzzlePiece");
        Image img = pieceGo.AddComponent<Image>();
        img.preserveAspect = false;
        img.raycastTarget = true;

        RectTransform rt = pieceGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(90, 90);

        pieceGo.AddComponent<PuzzlePiece>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pieceGo, prefabPath);
        Object.DestroyImmediate(pieceGo);

        Debug.Log($"[PuzzleSetupHelper] PuzzlePiece.prefab créé à {prefabPath}");
        return prefab;
    }
}
