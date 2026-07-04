using UnityEngine;

// Persistent Singleton that survives scene loads to hold match configuration.
// Stores the chosen player/enemy elements and provides quick access to affinity calculations.
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Affinity Reference")]
    [SerializeField] private AffinityData affinityData;

    public ElementType MainElement { get; private set; }
    public ElementType EnemyElement { get; private set; }
    
    // Debug/Cheats for testing
    public bool EnemyDetectionActive { get; private set; }
    public bool ForceMasterControl { get; private set; }

    public AffinityData AffinityData => affinityData;
    public bool HasChosenElement { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Player must explicitly choose an element in the menu before starting
        HasChosenElement = false;
        EnemyDetectionActive = false;
        ForceMasterControl = false; 

        if (affinityData == null)
            Debug.LogError("[GameSession] AffinityData is not assigned in the Inspector.", this);
    }

    public void SetMainElement(ElementType element)
    {
        MainElement = element;
        HasChosenElement = true;
    }

    public void SetEnemyElement(ElementType element)
    {
        EnemyElement = element;
    }

    // --- Affinity Helpers ---
    // These wrap the AffinityData lookups so callers don't need to pass the MainElement every time.

    public int GetAvailableAbilityCount(ElementType element)
    {
        if (affinityData == null) return 4;
        return affinityData.GetAvailableAbilityCount(MainElement, element);
    }

    public float GetEfficiency(ElementType element)
    {
        if (affinityData == null) return 1f;
        return affinityData.GetEfficiency(MainElement, element);
    }

    public float GetCooldownMultiplier(ElementType element)
    {
        if (affinityData == null) return 1f;
        return affinityData.GetCooldownMultiplier(MainElement, element);
    }

    // --- Debug / Cheat Toggles ---

    public void SetEnemyDetectionActive(bool isActive)
    {
        EnemyDetectionActive = isActive;
    }

    public void SetForceMasterControl(bool isActive)
    {
        ForceMasterControl = isActive;
    }

    // Clears session data when returning to the main menu
    public void ResetSession()
    {
        HasChosenElement = false;
        EnemyDetectionActive = false;
        ForceMasterControl = false;
        Time.timeScale = 1f; 
    }
}