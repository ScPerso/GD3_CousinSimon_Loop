using UnityEngine;
using UnityEditor;

public class CreateRechargeTile : MonoBehaviour
{
    [MenuItem("Assets/Create/Investigation/Recharge Tile")]
    public static void CreateAsset()
    {
        TileData asset = ScriptableObject.CreateInstance<TileData>();
        
        asset.tileName = "Station de recharge";
        asset.tileType = TileType.Recharge;
        asset.tileColor = new Color(0f, 1f, 1f, 1f);
        asset.isPassable = true;
        asset.canBeRevisited = true;
        asset.description = "Une station qui restaure vos ressources";

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Tiles/RechargeTile.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        Debug.Log("RechargeTile.asset créé avec succès!");
    }
}
