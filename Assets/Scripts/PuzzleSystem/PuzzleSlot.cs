using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Représente une case vide du plateau de puzzle (3x3).
/// Gère le feedback visuel de survol et accueille les pièces déposées.
/// </summary>
public class PuzzleSlot : MonoBehaviour
{
    [Header("Slot Configuration")]
    public int correctPieceIndex;

    [Header("Visual Feedback")]
    public Color defaultColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color hoverColor = new Color(0.7f, 0.85f, 1f, 1f);
    public Color correctColor = new Color(0.4f, 1f, 0.4f, 0.5f);

    private Image backgroundImage;
    private PuzzlePiece placedPiece;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = defaultColor;
        }
    }

    /// <summary>Active le feedback lumineux de survol.</summary>
    public void SetHoverHighlight(bool isHighlighted)
    {
        if (backgroundImage == null) return;

        if (isHighlighted)
        {
            backgroundImage.color = hoverColor;
        }
        else
        {
            backgroundImage.color = placedPiece != null ? defaultColor : defaultColor;
        }
    }

    /// <summary>Retourne vrai si la case est libre.</summary>
    public bool IsEmpty()
    {
        return placedPiece == null;
    }

    /// <summary>Place une pièce dans cette case.</summary>
    public void PlacePiece(PuzzlePiece piece)
    {
        if (placedPiece != null)
        {
            placedPiece.ReturnToOrigin();
        }

        placedPiece = piece;

        if (backgroundImage != null)
        {
            backgroundImage.color = defaultColor;
        }
    }

    /// <summary>Libère la case quand une pièce est retirée.</summary>
    public void ClearPiece()
    {
        placedPiece = null;

        if (backgroundImage != null)
        {
            backgroundImage.color = defaultColor;
        }
    }

    /// <summary>Retourne la pièce actuellement placée, ou null.</summary>
    public PuzzlePiece GetPlacedPiece()
    {
        return placedPiece;
    }

    /// <summary>Retourne vrai si la pièce placée est la bonne.</summary>
    public bool IsCorrect()
    {
        return placedPiece != null && placedPiece.pieceIndex == correctPieceIndex;
    }
}
