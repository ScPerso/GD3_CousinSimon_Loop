using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'état global du mini-jeu Dé à coudre :
/// début, victoire (25 cubes révélés sans retomber sur un visible), défaite, retour au jeu principal.
/// </summary>
public class ThimbleManager : MonoBehaviour
{
    public static ThimbleManager Instance { get; private set; }

    [Header("Scene")]
    public string mainSceneName = "SampleScene";

    [Header("UI")]
    public GameObject instructionPanel;
    public TextMeshProUGUI instructionText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI resultSubText;
    public Image resultBackground;

    [Header("Result Colors")]
    public Color victoryColor = new Color(0.15f, 0.75f, 0.15f, 0.92f);
    public Color defeatColor  = new Color(0.75f, 0.1f,  0.1f,  0.92f);

    [Header("Result Timings")]
    public float resultDisplayDuration = 3f;

    [Header("Resources")]
    public int rewardOnSuccess = 10;
    public int penaltyOnFailure = 10;

    [Header("Game Config")]
    [Tooltip("Nombre total de cubes à révéler pour gagner.")]
    public int totalCubes = 25;

    public bool IsPlaying { get; private set; }

    private int revealedCubes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        IsPlaying = true;
        revealedCubes = 0;

        if (resultPanel != null) resultPanel.SetActive(false);

        if (instructionText != null)
            instructionText.text = "Saute dans la piscine et révèle les <color=#FFD700>25 zones</color> pour gagner !\n" +
                                   "Esquive les zones <color=#FF4444>rouges</color> dévoilées — les retoucher = défaite.\n" +
                                   "<color=#FFD700>ZQSD</color> pour se déplacer · <color=#FFD700>Espace</color> pour sauter.";

        if (instructionPanel != null)
            StartCoroutine(HideInstructionAfterDelay(5f));
    }

    /// <summary>Appelé par ThimbleWaterCube quand un cube est révélé pour la première fois.</summary>
    public void OnCubeRevealed()
    {
        if (!IsPlaying) return;
        revealedCubes++;
        Debug.Log($"[ThimbleManager] Cube révélé {revealedCubes}/{totalCubes}.");

        if (revealedCubes >= totalCubes)
            TriggerVictory();
    }

    /// <summary>Appelé par ThimbleWaterCube quand le joueur atterrit sur un cube déjà visible.</summary>
    public void TriggerDefeat()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        Debug.Log("[ThimbleManager] DEFAITE — cube rouge touché !");
        GameManager.Instance?.SetMiniGameResult(false);
        ShowResult(false);
        StartCoroutine(ReturnToMain(resultDisplayDuration));
    }

    private void TriggerVictory()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        Debug.Log("[ThimbleManager] VICTOIRE — tous les cubes révélés !");
        GameManager.Instance?.SetMiniGameResult(true);
        ShowResult(true);
        StartCoroutine(ReturnToMain(resultDisplayDuration));
    }

    private void ShowResult(bool success)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        if (resultBackground != null)
            resultBackground.color = success ? victoryColor : defeatColor;

        if (resultText != null)
            resultText.text = success ? "RÉUSSI !" : "ÉLIMINÉ !";

        if (resultSubText != null)
        {
            int delta = success ? rewardOnSuccess : -penaltyOnFailure;
            resultSubText.text = delta >= 0
                ? $"<color=#90EE90>+{delta} Ressources</color>"
                : $"<color=#FF7777>{delta} Ressources</color>";
        }
    }

    private IEnumerator HideInstructionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (instructionPanel != null) instructionPanel.SetActive(false);
    }

    private IEnumerator ReturnToMain(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"[ThimbleManager] Retour vers '{mainSceneName}'.");
        SceneManager.LoadScene(mainSceneName);
    }
}
