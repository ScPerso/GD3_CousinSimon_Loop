using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Board Configuration")]
    public int boardSize = 7;
    public float tileSpacing = 1.1f;

    [Header("Tile Prefabs")]
    public GameObject tilePrefab;
    public TileData[] availableTileTypes;

    [Header("Generation Settings")]
    public bool generateOnStart = true;
    public int seed = 0;

    private Dictionary<Vector2Int, BoardTile> tileGrid = new Dictionary<Vector2Int, BoardTile>();
    private List<BoardTile> pathTiles = new List<BoardTile>();
    
    public int TotalTiles => pathTiles.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateBoard();
        }
    }

    public void GenerateBoard()
    {
        ClearBoard();

        if (seed != 0)
        {
            UnityEngine.Random.InitState(seed);
        }

        GenerateSquareLoop();
        EnsureSpecialTiles();

        Debug.Log($"Generated loop board: {pathTiles.Count} tiles in path");
    }

    private void GenerateSquareLoop()
    {
        pathTiles.Clear();
        int pathIndex = 0;

        Vector2Int[] loopPath = new Vector2Int[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0),
            new Vector2Int(4, 1),
            new Vector2Int(5, 1),
            new Vector2Int(6, 2),
            new Vector2Int(6, 3),
            new Vector2Int(6, 4),
            new Vector2Int(5, 5),
            new Vector2Int(4, 5),
            new Vector2Int(3, 6),
            new Vector2Int(2, 6),
            new Vector2Int(1, 6),
            new Vector2Int(0, 5),
            new Vector2Int(0, 4),
            new Vector2Int(0, 3),
            new Vector2Int(0, 2),
            new Vector2Int(0, 1)
        };

        foreach (Vector2Int gridPos in loopPath)
        {
            CreatePathTile(gridPos, pathIndex++);
        }
    }

    private void CreatePathTile(Vector2Int gridPosition, int pathIndex)
    {
        Vector3 worldPosition = new Vector3(gridPosition.x * tileSpacing, 0, gridPosition.y * tileSpacing);
        
        GameObject tileObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
        tileObject.name = $"Tile_{pathIndex:D2}_{gridPosition.x}_{gridPosition.y}";

        BoardTile tile = tileObject.GetComponent<BoardTile>();
        if (tile != null)
        {
            tile.gridPosition = gridPosition;
            tile.pathIndex = pathIndex;
            tile.tileData = GetRandomTileData();
            tile.InitializeTile();
            
            tileGrid[gridPosition] = tile;
            pathTiles.Add(tile);
        }
    }

    private TileData GetRandomTileData()
    {
        if (availableTileTypes == null || availableTileTypes.Length == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, availableTileTypes.Length);
        return availableTileTypes[randomIndex];
    }

    public void ClearBoard()
    {
        foreach (var tile in tileGrid.Values)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }

        tileGrid.Clear();
        pathTiles.Clear();
    }

    public BoardTile GetTileAt(Vector2Int position)
    {
        tileGrid.TryGetValue(position, out BoardTile tile);
        return tile;
    }

    public BoardTile GetTileByPathIndex(int index)
    {
        if (index < 0 || index >= pathTiles.Count)
            return null;
        
        return pathTiles[index];
    }

    public int GetNextPathIndex(int currentIndex, int steps)
    {
        if (pathTiles.Count == 0)
            return 0;
        
        return (currentIndex + steps) % pathTiles.Count;
    }

    public List<BoardTile> GetNeighbors(Vector2Int position)
    {
        List<BoardTile> neighbors = new List<BoardTile>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = position + dir;
            BoardTile neighbor = GetTileAt(neighborPos);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public List<BoardTile> GetTilesByType(TileType type)
    {
        List<BoardTile> tiles = new List<BoardTile>();

        foreach (BoardTile tile in tileGrid.Values)
        {
            if (tile.tileData != null && tile.tileData.tileType == type)
            {
                tiles.Add(tile);
            }
        }

        return tiles;
    }

    public void EnsureSpecialTiles()
    {
        int totalTiles = pathTiles.Count;
        List<int> usedIndices = new List<int>();
        
        PlaceTileAtIndex(0, GetTileDataByType(TileType.Captain), usedIndices);
        PlaceMultipleTilesWithSpacing(1, GetTileDataByType(TileType.WrittenNote), usedIndices, 3, totalTiles);
        PlaceMultipleTilesWithSpacing(1, GetTileDataByType(TileType.PersonalItem), usedIndices, 3, totalTiles);
        PlaceMultipleTilesWithSpacing(1, GetTileDataByType(TileType.Evidence), usedIndices, 3, totalTiles);
        PlaceMultipleTilesWithSpacing(1, GetTileDataByType(TileType.Corpse), usedIndices, 3, totalTiles);
        PlaceMultipleTilesWithSpacing(2, GetTileDataByType(TileType.Recharge), usedIndices, 3, totalTiles);
        
        FillRemainingWithEmpty(usedIndices);
        
        Debug.Log($"Special tiles placed: 1 Captain (index 0), 1 Note, 1 Item, 1 Evidence, 1 Corpse, 2 Recharge, {totalTiles - usedIndices.Count} Empty");
    }
    
    private void PlaceMultipleTilesWithSpacing(int count, TileData tileData, List<int> usedIndices, int minSpacing, int totalTiles)
    {
        if (tileData == null || count <= 0) return;
        
        int spacing = totalTiles / (count + 1);
        
        for (int i = 0; i < count; i++)
        {
            int preferredIndex = spacing * (i + 1);
            int finalIndex = preferredIndex;
            
            bool alreadyUsed = usedIndices.Contains(preferredIndex);
            bool tooClose = IsIndexTooCloseToUsed(preferredIndex, usedIndices, minSpacing, totalTiles);
            
            if (alreadyUsed || tooClose)
            {
                Debug.Log($"Index {preferredIndex} for {tileData.tileType} unavailable (used:{alreadyUsed}, tooClose:{tooClose}). Finding alternative...");
                finalIndex = FindValidIndexNear(preferredIndex, usedIndices, minSpacing, totalTiles);
                Debug.Log($"Found alternative index {finalIndex} for {tileData.tileType}");
            }
            
            usedIndices.Add(finalIndex);
            PlaceSpecificTileAtPath(finalIndex, tileData);
        }
    }
    
    private void PlaceTileAtIndex(int index, TileData tileData, List<int> usedIndices)
    {
        if (tileData == null || index < 0 || index >= pathTiles.Count) return;
        
        usedIndices.Add(index);
        PlaceSpecificTileAtPath(index, tileData);
    }
    
    private void FillRemainingWithEmpty(List<int> usedIndices)
    {
        TileData emptyTileData = GetTileDataByType(TileType.Empty);
        if (emptyTileData == null) return;
        
        for (int i = 0; i < pathTiles.Count; i++)
        {
            if (!usedIndices.Contains(i))
            {
                PlaceSpecificTileAtPath(i, emptyTileData);
            }
        }
    }
    
    private bool IsIndexTooCloseToUsed(int index, List<int> usedIndices, int minSpacing, int totalTiles)
    {
        foreach (int used in usedIndices)
        {
            int distance = Mathf.Min(
                Mathf.Abs(index - used),
                totalTiles - Mathf.Abs(index - used)
            );
            
            if (distance < minSpacing)
            {
                return true;
            }
        }
        return false;
    }
    
    private int FindValidIndexNear(int preferredIndex, List<int> usedIndices, int minSpacing, int totalTiles)
    {
        for (int reducedSpacing = minSpacing; reducedSpacing >= 0; reducedSpacing--)
        {
            for (int offset = 1; offset < totalTiles; offset++)
            {
                int candidate1 = (preferredIndex + offset) % totalTiles;
                
                if (!usedIndices.Contains(candidate1) && !IsIndexTooCloseToUsed(candidate1, usedIndices, reducedSpacing, totalTiles))
                {
                    if (reducedSpacing < minSpacing)
                    {
                        Debug.LogWarning($"FindValidIndexNear: Found index {candidate1} with REDUCED spacing {reducedSpacing} (original was {minSpacing})");
                    }
                    return candidate1;
                }
                
                int candidate2 = (preferredIndex - offset + totalTiles) % totalTiles;
                
                if (!usedIndices.Contains(candidate2) && !IsIndexTooCloseToUsed(candidate2, usedIndices, reducedSpacing, totalTiles))
                {
                    if (reducedSpacing < minSpacing)
                    {
                        Debug.LogWarning($"FindValidIndexNear: Found index {candidate2} with REDUCED spacing {reducedSpacing} (original was {minSpacing})");
                    }
                    return candidate2;
                }
            }
        }
        
        Debug.LogError($"FindValidIndexNear: CRITICAL - NO VALID INDEX FOUND EVEN WITH ZERO SPACING! This should never happen. Returning preferredIndex {preferredIndex}");
        return preferredIndex;
    }

    private void PlaceSpecificTileAtPath(int pathIndex, TileData tileData)
    {
        BoardTile tile = GetTileByPathIndex(pathIndex);
        if (tile != null && tileData != null)
        {
            tile.tileData = tileData;
            tile.InitializeTile();
            
            Debug.Log($"Placed {tileData.tileType} at path index {pathIndex}");
        }
    }

    private TileData GetTileDataByType(TileType type)
    {
        if (availableTileTypes == null)
            return null;

        foreach (TileData data in availableTileTypes)
        {
            if (data != null && data.tileType == type)
                return data;
        }
        
        Debug.LogWarning($"No TileData found for type: {type}");
        return null;
    }
}
