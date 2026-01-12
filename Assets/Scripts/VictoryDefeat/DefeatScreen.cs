using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DefeatScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject defeatPanel;
    public TextMeshProUGUI defeatTitleText;
    public TextMeshProUGUI defeatMessageText;
    public TextMeshProUGUI reasonText;
    public Button restartButton;
    public Button quitButton;

    [Header("Animation")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 1f;

    [Header("Audio")]
    public AudioClip defeatMusic;
    public AudioClip defeatSound;

    [Header("Defeat Messages")]
    public string defeatTitle = "DEFEAT";
    public string resourcesDepletedMessage = "Your resources have been depleted...";

    [Header("Board Visibility")]
    public bool hideBoardOnDefeat = true;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        if (canvasGroup == null && defeatPanel != null)
        {
            canvasGroup = defeatPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = defeatPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnDefeat += ShowDefeatScreen;
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void ShowDefeatScreen()
    {
        if (hideBoardOnDefeat && BoardManager.Instance != null)
        {
            BoardManager.Instance.gameObject.SetActive(false);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        UpdateDefeatText();
        DetermineDefeatReason();

        if (defeatSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(defeatSound);
        }

        if (defeatMusic != null && audioSource != null)
        {
            audioSource.clip = defeatMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        StartCoroutine(FadeIn());
    }

    private void UpdateDefeatText()
    {
        if (defeatTitleText != null)
        {
            defeatTitleText.text = defeatTitle;
        }

        if (defeatMessageText != null)
        {
            defeatMessageText.text = "L'enquête s'arrête ici...";
        }
    }

    private void DetermineDefeatReason()
    {
        if (reasonText == null)
            return;

        string reason = "";

        if (ResourceManager.Instance != null && ResourceManager.Instance.CurrentResources <= 0)
        {
            reason = $"<color=#FF0000>Ressources épuisées ! Vous n'avez pas survécu assez longtemps pour terminer l'enquête.</color>";
        }
        else
        {
            reason = "Vous n'avez pas réussi à résoudre l'enquête.";
        }

        if (PlayerLoopController.Instance != null)
        {
            reason += $"\n\n<color=#FFD700>Vous avez survécu {PlayerLoopController.Instance.TotalLoops} boucles.</color>";
        }

        if (GameManager.Instance != null)
        {
            reason += "\n\n<b>PREUVES COLLECTÉES</b>\n";
            
            if (GameManager.Instance.HasFlag("clue_note_collected"))
                reason += "✓ Note écrite trouvée\n";
            else
                reason += "○ Note écrite manquante\n";
            
            if (GameManager.Instance.HasFlag("clue_item_collected"))
                reason += "✓ Objet personnel trouvé\n";
            else
                reason += "○ Objet personnel manquant\n";
            
            if (GameManager.Instance.HasFlag("clue_evidence_collected"))
                reason += "✓ Preuves matérielles trouvées\n";
            else
                reason += "○ Preuves matérielles manquantes\n";
            
            if (GameManager.Instance.HasFlag("clue_corpse_found"))
                reason += "✓ Cadavre découvert\n";
            else
                reason += "○ Cadavre non découvert\n";
        }

        reasonText.text = reason;
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null)
            yield break;

        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void OnRestartClicked()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RestartGame();
        }
    }

    private void OnQuitClicked()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.QuitGame();
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnDefeat -= ShowDefeatScreen;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}
