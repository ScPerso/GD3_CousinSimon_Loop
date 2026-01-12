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
