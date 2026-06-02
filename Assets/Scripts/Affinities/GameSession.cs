using UnityEngine;

/// <summary>
/// Persistent Singleton that manages the current game session state,
/// including the chosen player/enemy elements and affinity data.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Affinity Reference")]
    [SerializeField] private AffinityData affinityData;

    public ElementType MainElement { get; private set; }
    public ElementType EnemyElement { get; private set; }
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

        // No default element — the player MUST choose one in the ElementSelector.
        HasChosenElement = false;

        if (affinityData == null)
            Debug.LogError("[GameSession] AffinityData is not assigned in the Inspector.", this);
    }

    /// <summary>
    /// Sets the player's main element and marks the selection as complete.
    /// </summary>
    public void SetMainElement(ElementType element)
    {
        MainElement = element;
        HasChosenElement = true;
        Debug.Log($"[GameSession] Main element set to: {element}");
    }

    /// <summary>
    /// Sets the enemy's element for the current match.
    /// </summary>
    public void SetEnemyElement(ElementType element)
    {
        EnemyElement = element;
        Debug.Log($"[GameSession] Enemy element set to: {element}");
    }

    /// <summary>
    /// Returns the number of abilities available for a specific element based on affinity.
    /// </summary>
    public int GetAvailableAbilityCount(ElementType element)
    {
        if (affinityData == null) return 4;
        return affinityData.GetAvailableAbilityCount(MainElement, element);
    }

    /// <summary>
    /// Returns the damage efficiency multiplier for a specific element based on affinity.
    /// </summary>
    public float GetEfficiency(ElementType element)
    {
        if (affinityData == null) return 1f;
        return affinityData.GetEfficiency(MainElement, element);
    }

    /// <summary>
    /// Returns the cooldown multiplier for a specific element based on affinity.
    /// </summary>
    public float GetCooldownMultiplier(ElementType element)
    {
        if (affinityData == null) return 1f;
        return affinityData.GetCooldownMultiplier(MainElement, element);
    }

    /// <summary>
    /// Resets the session state, requiring the player to choose an element again.
    /// </summary>
    public void ResetSession()
    {
        HasChosenElement = false;
    }
}