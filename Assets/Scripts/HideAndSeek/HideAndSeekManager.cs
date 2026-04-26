using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'état global du mini-jeu cache-cache : début, victoire, défaite, retour au jeu principal.
/// </summary>
public class HideAndSeekManager : MonoBehaviour
{
    public static HideAndSeekManager Instance { get; private set; }

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

    [Header("Resources (PuzzleBridge pattern)")]
    public int rewardOnSuccess = 10;
    public int penaltyOnFailure = -10;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Debug.Log("[HideAndSeekManager] Awake OK.");
    }

    private void OnDestroy() { }

    private void Start()
    {
        IsPlaying = true;

        if (resultPanel != null) resultPanel.SetActive(false);

        if (instructionText != null)
            instructionText.text = "Rejoins le point <color=#FFD700>B</color> sans te faire attraper !\nUtilise <color=#FFD700>ZQSD</color> pour te déplacer.";

        if (instructionPanel != null)
            StartCoroutine(HideInstructionAfterDelay(4f));

        Debug.Log("[HideAndSeekManager] Mini-jeu démarré.");
    }

    private IEnumerator HideInstructionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
            Debug.Log("[HideAndSeekManager] Panel instruction masqué.");
        }
    }

    /// <summary>Appelé par la zone Goal quand le joueur l'atteint.</summary>
    public void TriggerVictory()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        Debug.Log("[HideAndSeekManager] VICTOIRE !");
        GameManager.Instance?.SetMiniGameResult(true);
        ShowResult(true);
        StartCoroutine(ReturnToMain(resultDisplayDuration));
    }

    /// <summary>Appelé par l'IA quand elle attrape le joueur.</summary>
    public void TriggerDefeat()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        Debug.Log("[HideAndSeekManager] DEFAITE !");
        GameManager.Instance?.SetMiniGameResult(false);
        ShowResult(false);
        StartCoroutine(ReturnToMain(resultDisplayDuration));
    }

    private void ShowResult(bool success)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        if (resultBackground != null)
            resultBackground.color = success ? victoryColor : defeatColor;

        if (resultText != null)
            resultText.text = success ? "RÉUSSI !" : "ATTRAPÉ !";

        if (resultSubText != null)
        {
            int delta = success ? rewardOnSuccess : penaltyOnFailure;
            resultSubText.text = delta >= 0
                ? $"<color=#90EE90>+{delta} Ressources</color>"
                : $"<color=#FF7777>{delta} Ressources</color>";
        }
    }

    private IEnumerator ReturnToMain(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"[HideAndSeekManager] Retour vers '{mainSceneName}'.");
        SceneManager.LoadScene(mainSceneName);
    }
}
