using UnityEditor;
using UnityEngine;

public class CreatePuzzleTile : MonoBehaviour
{
    [MenuItem("Assets/Create/Investigation/Puzzle Tile")]
    public static void CreatePuzzleTileAsset()
    {
        TileData asset = ScriptableObject.CreateInstance<TileData>();
        asset.tileName = "Puzzle mystérieux";
        asset.tileType = TileType.Puzzle;
        asset.description = "Un puzzle étrange qui demande votre attention. Résolvez-le pour gagner des ressources.";
        asset.tileColor = new Color(0.6f, 0.2f, 0.8f, 1f);
        asset.isPassable = true;
        asset.triggersOnEnter = true;
        asset.canBeRevisited = true;

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Tiles/PuzzleTile.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("PuzzleTile.asset créé avec succès!");
    }
}
