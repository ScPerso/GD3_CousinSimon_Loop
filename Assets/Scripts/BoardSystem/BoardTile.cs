using System;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [Header("Tile Configuration")]
    public TileData tileData;
    public Vector2Int gridPosition;
    public int pathIndex;

    [Header("State")]
    public bool isVisited;
    public bool isOccupied;
    public bool isActive = true;

    [Header("Visual Components")]
    public SpriteRenderer tileRenderer;
    public MeshRenderer meshRenderer;
    public GameObject selectionIndicator;
    public ParticleSystem activationEffect;

    public event Action<BoardTile> OnTileEntered;
    public event Action<BoardTile> OnTileExited;
    public event Action<BoardTile> OnTileActivated;

    public TileData TileData => tileData;
    public bool HasBeenVisited => isVisited;

    private Material tileMaterial;
    private Color originalColor;

    private void Awake()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<SpriteRenderer>();
        }

        if (meshRenderer != null)
        {
            tileMaterial = meshRenderer.material;
            originalColor = meshRenderer.material.color;
        }
        else if (tileRenderer != null)
        {
            tileMaterial = tileRenderer.material;
            originalColor = tileRenderer.color;
        }

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    private void Start()
    {
        InitializeTile();
    }

    public void InitializeTile()
    {
        if (tileData == null)
            return;

        if (meshRenderer != null)
        {
            Material mat = meshRenderer.material;
            mat.color = tileData.tileColor;
            originalColor = tileData.tileColor;
        }
        else if (tileRenderer != null)
        {
            if (tileData.tileIcon != null)
            {
                tileRenderer.sprite = tileData.tileIcon;
            }
            tileRenderer.color = tileData.tileColor;
            originalColor = tileData.tileColor;
        }

        if (tileData.tileType == TileType.Thimble)
            SpawnThimbleDecorations();
    }

    /// <summary>
    /// Spawn 2 petits cubes rouges décoratifs sans collider au-dessus de la case,
    /// pour évoquer une piscine avec des cubes rouges posés dessus.
    /// </summary>
    private void SpawnThimbleDecorations()
    {
        // Positions locales légèrement décalées pour un aspect naturel
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-0.18f, 0.58f,  0.12f),
            new Vector3( 0.15f, 0.58f, -0.10f)
        };

        foreach (Vector3 offset in offsets)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "ThimbleDecor";
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = offset;
            cube.transform.localScale    = new Vector3(0.18f, 0.18f, 0.18f);

            // Couleur rouge
            MeshRenderer r = cube.GetComponent<MeshRenderer>();
            if (r != null)
            {
                r.material = new Material(r.sharedMaterial);
                r.material.color = new Color(0.85f, 0.1f, 0.1f);
            }

            // Pas de collision
            Collider col = cube.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
        }
    }

    public void EnterTile(GameObject entity)
    {
        if (!isActive || !tileData.isPassable)
            return;

        isOccupied = true;
        isVisited = true;
        OnTileEntered?.Invoke(this);
    }

    public void ExitTile(GameObject entity)
    {
        isOccupied = false;
        OnTileExited?.Invoke(this);
    }

    public void ActivateTile(GameObject activator)
    {
        if (!isActive)
            return;

        ExecuteTileAction(activator);
        PlayActivationEffect();
        OnTileActivated?.Invoke(this);
    }

    private void ExecuteTileAction(GameObject activator)
    {
        switch (tileData.tileType)
        {
            case TileType.Empty:
                HandleEmptyTile(activator);
                break;
            case TileType.Captain:
                HandleCaptainTile(activator);
                break;
            case TileType.WrittenNote:
                HandleWrittenNoteTile(activator);
                break;
            case TileType.PersonalItem:
                HandlePersonalItemTile(activator);
                break;
            case TileType.Evidence:
                HandleEvidenceTile(activator);
                break;
            case TileType.Corpse:
                HandleCorpseTile(activator);
                break;
            case TileType.Recharge:
                HandleRechargeTile(activator);
                break;
            case TileType.HideAndSeek:
                HandleHideAndSeekTile(activator);
                break;
            case TileType.Puzzle:
                HandlePuzzleTile(activator);
                break;
            case TileType.Thimble:
                HandleThimbleTile(activator);
                break;
        }
    }

    private void HandleEmptyTile(GameObject activator)
    {
        Debug.Log($"Stepped on empty tile at {gridPosition}");
    }

    private void HandleCaptainTile(GameObject activator)
    {
        Debug.Log($"Rencontre avec le Capitaine à {gridPosition}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlag($"captain_visited_{gridPosition.x}_{gridPosition.y}");
        }
    }

    private void HandleWrittenNoteTile(GameObject activator)
    {
        Debug.Log($"Note découverte à {gridPosition}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlag($"note_found_{gridPosition.x}_{gridPosition.y}");
            GameManager.Instance.AddFlag("clue_note_collected");
        }
    }

    private void HandlePersonalItemTile(GameObject activator)
    {
        Debug.Log($"Objet personnel découvert à {gridPosition}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlag($"item_found_{gridPosition.x}_{gridPosition.y}");
            GameManager.Instance.AddFlag("clue_item_collected");
        }
    }

    private void HandleEvidenceTile(GameObject activator)
    {
        Debug.Log($"Preuve découverte à {gridPosition}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlag($"evidence_found_{gridPosition.x}_{gridPosition.y}");
            GameManager.Instance.AddFlag("clue_evidence_collected");
        }
    }

    private void HandleCorpseTile(GameObject activator)
    {
        Debug.Log($"Cadavre découvert à {gridPosition}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlag($"corpse_found_{gridPosition.x}_{gridPosition.y}");
            GameManager.Instance.AddFlag("clue_corpse_found");
        }
    }

    private void HandleRechargeTile(GameObject activator)
    {
        Debug.Log($"Case de recharge activée à {gridPosition}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddFlag($"recharge_visited_{gridPosition.x}_{gridPosition.y}");
        }
    }

    private void HandleHideAndSeekTile(GameObject activator)
    {
        Debug.Log($"[BoardTile] Case HideAndSeek activée à {gridPosition}");
        PlayerLoopController.Instance?.ForceEndTurnForSceneChange(10, 10);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Mini-Jeu1");
    }

    private void HandlePuzzleTile(GameObject activator)
    {
        Debug.Log($"[BoardTile] Case Puzzle activée à {gridPosition}");
        PlayerLoopController.Instance?.ForceEndTurnForSceneChange(10, 10);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Mini-Jeu-Puzzle");
    }

    private void HandleThimbleTile(GameObject activator)
    {
        Debug.Log($"[BoardTile] Case Dé à coudre activée à {gridPosition}");
        PlayerLoopController.Instance?.ForceEndTurnForSceneChange(10, 10);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Mini-DeACoudre");
    }

    private void PlayActivationEffect()
    {
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        if (tileData.actionSound != null)
        {
            AudioSource.PlayClipAtPoint(tileData.actionSound, transform.position);
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(highlighted);
        }

        Color highlightColor = highlighted ? Color.Lerp(originalColor, Color.white, 0.5f) : originalColor;
        
        if (meshRenderer != null)
        {
            meshRenderer.material.color = highlightColor;
        }
        else if (tileRenderer != null)
        {
            tileRenderer.color = highlightColor;
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;

        Color targetColor = active ? originalColor : originalColor * 0.5f;
        
        if (meshRenderer != null)
        {
            meshRenderer.material.color = targetColor;
        }
        else if (tileRenderer != null)
        {
            tileRenderer.color = targetColor;
        }
    }

    public bool CanBeEntered()
    {
        if (!isActive || tileData == null || !tileData.isPassable)
            return false;

        return true;
    }

    public void MarkAsVisited()
    {
        isVisited = true;
    }

    public string GetTileInfo()
    {
        return $"{tileData.tileName}\n{tileData.description}\nPosition: {gridPosition}";
    }

    private void OnMouseEnter()
    {
        SetHighlighted(true);
    }

    private void OnMouseExit()
    {
        SetHighlighted(false);
    }

    private void OnMouseDown()
    {
        Debug.Log(GetTileInfo());
    }
}
