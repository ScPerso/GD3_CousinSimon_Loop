using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pièce de puzzle draggable. Le rootCanvas est résolu lazily au premier drag
/// pour éviter les problèmes d'ordre d'initialisation après Instantiate.
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>Index de la pièce (1 à 9), correspond au sprite attendu dans ce slot.</summary>
    public int pieceIndex;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Vector2 originAnchorMin;
    private Vector2 originAnchorMax;
    private Vector2 originSizeDelta;
    private Vector2 originPosition;
    private Transform originParent;
    private int originSiblingIndex;

    private PuzzleSlot currentSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>Résolution lazy du Canvas racine — appelée au premier drag pour garantir que le GO est bien en scène.</summary>
    private Canvas GetRootCanvas()
    {
        if (rootCanvas != null) return rootCanvas;

        Canvas[] canvases = GetComponentsInParent<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (c.isRootCanvas)
            {
                rootCanvas = c;
                return rootCanvas;
            }
        }

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
            Debug.LogError($"[PuzzlePiece] '{name}' : aucun Canvas parent trouvé ! Vérifie que le prefab est bien enfant d'un Canvas.");

        return rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Canvas canvas = GetRootCanvas();
        if (canvas == null) return;

        if (currentSlot != null)
        {
            currentSlot.ClearPiece();
            currentSlot = null;
        }

        // Sauvegarder l'état complet avant tout déplacement
        originParent = transform.parent;
        originSiblingIndex = transform.GetSiblingIndex();
        originAnchorMin = rectTransform.anchorMin;
        originAnchorMax = rectTransform.anchorMax;
        originSizeDelta = rectTransform.sizeDelta;
        originPosition = rectTransform.anchoredPosition;

        // Remonter au Canvas racine pour passer visuellement par-dessus tout
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;

        Debug.Log($"[PuzzlePiece] BeginDrag pieceIndex={pieceIndex}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Canvas canvas = GetRootCanvas();
        if (canvas == null) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        HighlightSlotUnderCursor(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        ClearAllHighlights();

        PuzzleSlot targetSlot = GetSlotUnderCursor(eventData);
        Debug.Log($"[PuzzlePiece] EndDrag pieceIndex={pieceIndex} → slot={( targetSlot != null ? targetSlot.name : "aucun")}");

        if (targetSlot != null)
            PlaceInSlot(targetSlot);
        else
            ReturnToOrigin();

        PuzzleSceneManager.Instance?.CheckPuzzleCompletion();
    }

    private void HighlightSlotUnderCursor(PointerEventData eventData)
    {
        ClearAllHighlights();
        GetSlotUnderCursor(eventData)?.SetHoverHighlight(true);
    }

    private void ClearAllHighlights()
    {
        foreach (PuzzleSlot slot in FindObjectsByType<PuzzleSlot>(FindObjectsSortMode.None))
            slot.SetHoverHighlight(false);
    }

    private PuzzleSlot GetSlotUnderCursor(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            PuzzleSlot slot = result.gameObject.GetComponent<PuzzleSlot>();
            if (slot != null) return slot;
        }

        return null;
    }

    private void PlaceInSlot(PuzzleSlot targetSlot)
    {
        // Si le slot contient déjà une pièce, la renvoyer à son origine avant de placer la nouvelle
        targetSlot.PlacePiece(this);
        currentSlot = targetSlot;

        transform.SetParent(targetSlot.transform, false);

        // Étirer la pièce pour couvrir exactement le slot — ancres (0,0)→(1,1), offsets zéro
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Debug.Log($"[PuzzlePiece] Pièce {pieceIndex} → '{targetSlot.name}' (attendu {targetSlot.correctPieceIndex}) : {(targetSlot.IsCorrect() ? "CORRECT ✓" : "faux")}");
    }

    /// <summary>Retourne la pièce à sa position d'origine dans la barre du bas.</summary>
    public void ReturnToOrigin()
    {
        if (currentSlot != null)
        {
            currentSlot.ClearPiece();
            currentSlot = null;
        }

        transform.SetParent(originParent, false);
        transform.SetSiblingIndex(originSiblingIndex);

        // Restaurer exactement l'état d'origine (ancres + taille + position)
        rectTransform.anchorMin = originAnchorMin;
        rectTransform.anchorMax = originAnchorMax;
        rectTransform.sizeDelta = originSizeDelta;
        rectTransform.anchoredPosition = originPosition;
    }
}
