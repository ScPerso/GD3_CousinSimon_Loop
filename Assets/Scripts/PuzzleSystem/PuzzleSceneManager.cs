using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère la scène du mini-jeu puzzle : timer 20s, spawn des pièces, vérification, retour LoopHero.
/// </summary>
public class PuzzleSceneManager : MonoBehaviour
{
    public static PuzzleSceneManager Instance { get; private set; }

    [Header("Scene")]
    public string loopHeroSceneName = "SampleScene";

    [Header("Puzzle Data")]
    public Sprite[] puzzleSprites;

    [Header("UI References - Slots (3x3)")]
    public PuzzleSlot[] slots;

    [Header("UI References - Piece Tray")]
    public Transform pieceTray;
    public GameObject piecePrefab;

    [Header("UI References - Timer")]
    public TextMeshProUGUI timerText;

    [Header("UI References - Feedback")]
    public GameObject resultPanel;
    public Image resultPanelBackground;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI resourceEffectText;

    [Header("Timer Settings")]
    public float timerDuration = 30f;

    [Header("Result Colors")]
    public Color successColor = new Color(0.2f, 0.8f, 0.2f);
    public Color failureColor = new Color(0.8f, 0.2f, 0.2f);
    public Color timerWarningColor = new Color(1f, 0.4f, 0.1f);
    public Color timerNormalColor = Color.white;

    private const float WarningThreshold = 7f;

    private bool puzzleCompleted;
    private float timeRemaining;
    private bool timerRunning;

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
        if (resultPanel != null)
            resultPanel.SetActive(false);

        SpawnPuzzlePieces();
        AssignCorrectPieceIndices();

        timeRemaining = timerDuration;
        timerRunning = true;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!timerRunning || puzzleCompleted) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            UpdateTimerDisplay();
            StartCoroutine(ShowResultAndReturn(false));
            return;
        }

        UpdateTimerDisplay();
    }

    /// <summary>Met à jour l'affichage du timer et change la couleur sous le seuil d'alerte.</summary>
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds.ToString();
        timerText.color = timeRemaining <= WarningThreshold ? timerWarningColor : timerNormalColor;
    }

    /// <summary>Crée les pièces dans la barre du bas dans un ordre aléatoire.</summary>
    private void SpawnPuzzlePieces()
    {
        if (pieceTray == null || piecePrefab == null || puzzleSprites == null) return;

        int[] order = ShuffledOrder(puzzleSprites.Length);

        for (int i = 0; i < puzzleSprites.Length; i++)
        {
            int spriteIndex = order[i];
            GameObject pieceGo = Instantiate(piecePrefab, pieceTray);

            PuzzlePiece piece = pieceGo.GetComponent<PuzzlePiece>();
            if (piece != null)
                piece.pieceIndex = spriteIndex + 1;

            Image pieceImage = pieceGo.GetComponent<Image>();
            if (pieceImage != null && spriteIndex < puzzleSprites.Length)
            {
                pieceImage.sprite = puzzleSprites[spriteIndex];
                pieceImage.preserveAspect = false;
            }
        }
    }

    /// <summary>Assigne l'index correct à chaque slot (1–9, gauche→droite, haut→bas).</summary>
    private void AssignCorrectPieceIndices()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].correctPieceIndex = i + 1;
        }
    }

    /// <summary>Génère un tableau d'indices mélangés.</summary>
    private int[] ShuffledOrder(int count)
    {
        int[] indices = new int[count];
        for (int i = 0; i < count; i++) indices[i] = i;

        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        return indices;
    }

    /// <summary>Appelée après chaque dépôt de pièce. Vérifie si le puzzle est entièrement correct.</summary>
    public void CheckPuzzleCompletion()
    {
        if (puzzleCompleted || !timerRunning || slots == null) return;

        foreach (PuzzleSlot slot in slots)
        {
            if (slot == null || slot.IsEmpty() || !slot.IsCorrect())
                return;
        }

        timerRunning = false;
        puzzleCompleted = true;
        Debug.Log("[Puzzle] Puzzle complété avec succès !");
        StartCoroutine(ShowResultAndReturn(true));
    }

    private IEnumerator ShowResultAndReturn(bool success)
    {
        // Enregistrer le resultat dans GameManager (qui a deja la sauvegarde de la case et des ressources)
        GameManager.Instance?.SetMiniGameResult(success);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultPanelBackground != null)
            resultPanelBackground.color = success
                ? new Color(0f, 0.25f, 0f, 0.92f)
                : new Color(0.25f, 0f, 0f, 0.92f);

        if (resultText != null)
        {
            resultText.text = success ? "Puzzle resolu !" : "Temps ecoule...";
            resultText.color = success ? successColor : failureColor;
        }

        if (resourceEffectText != null)
        {
            resourceEffectText.text = success
                ? $"+{PuzzleBridge.RewardOnSuccess} Ressources"
                : $"{PuzzleBridge.PenaltyOnFailure} Ressources";
            resourceEffectText.color = success ? successColor : failureColor;
        }

        yield return new WaitForSeconds(2.5f);

        SceneManager.LoadScene(loopHeroSceneName);
    }
}
