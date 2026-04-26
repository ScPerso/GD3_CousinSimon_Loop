using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Starting Resources")]
    public int startingResources = 100;

    [Header("Critical Thresholds")]
    public int criticalThreshold = 20;
    public int warningThreshold = 50;

    public int CurrentResources { get; private set; }

    public event Action<int> OnResourcesChanged;
    public event Action OnResourcesDepleted;
    public event Action OnResourcesCritical;
    public event Action OnResourcesWarning;

    private bool hasCriticalWarningFired;
    private bool hasWarningFired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialiser dans Awake pour que GameStateManager.Start() trouve une valeur valide.
        // Le chargement apres mini-jeu est gere par PlayerLoopController.InitializePlayerAfterBoard().
        CurrentResources = startingResources;
    }

    private void Start()
    {
        OnResourcesChanged?.Invoke(CurrentResources);
    }

    /// <summary>Force les ressources a une valeur precise (utilise au chargement apres mini-jeu).</summary>
    public void SetResources(int amount)
    {
        CurrentResources = Mathf.Max(0, amount);
        hasCriticalWarningFired = false;
        hasWarningFired = false;
        OnResourcesChanged?.Invoke(CurrentResources);
        Debug.Log($"[ResourceManager] Ressources forcees a {CurrentResources}");
    }

    public void AddResources(int amount)
    {
        if (amount <= 0) return;
        CurrentResources += amount;
        OnResourcesChanged?.Invoke(CurrentResources);
        Debug.Log($"Ressources : +{amount} → {CurrentResources}");
    }

    public void RemoveResources(int amount)
    {
        if (amount <= 0) return;
        CurrentResources -= amount;
        CurrentResources = Mathf.Max(0, CurrentResources);
        OnResourcesChanged?.Invoke(CurrentResources);
        CheckResourceThresholds();
        Debug.Log($"Ressources : -{amount} → {CurrentResources}");
    }

    public bool HasEnoughResources(int amount) => CurrentResources >= amount;

    public bool TrySpendResources(int amount)
    {
        if (!HasEnoughResources(amount)) return false;
        RemoveResources(amount);
        return true;
    }

    private void CheckResourceThresholds()
    {
        if (CurrentResources <= 0)
        {
            OnResourcesDepleted?.Invoke();
            Debug.LogWarning("Ressources epuisees !");
        }
        else if (CurrentResources <= criticalThreshold && !hasCriticalWarningFired)
        {
            hasCriticalWarningFired = true;
            OnResourcesCritical?.Invoke();
        }
        else if (CurrentResources <= warningThreshold && !hasWarningFired)
        {
            hasWarningFired = true;
            OnResourcesWarning?.Invoke();
        }

        if (CurrentResources > criticalThreshold) hasCriticalWarningFired = false;
        if (CurrentResources > warningThreshold) hasWarningFired = false;
    }

    public void ResetResources()
    {
        CurrentResources = startingResources;
        hasCriticalWarningFired = false;
        hasWarningFired = false;
        OnResourcesChanged?.Invoke(CurrentResources);
    }
}
