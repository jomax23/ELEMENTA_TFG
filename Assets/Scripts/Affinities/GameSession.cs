using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Affinity Reference")]
    [SerializeField] private AffinityData affinityData;

    public ElementType MainElement   { get; private set; }
    public AffinityData AffinityData => affinityData;
    public bool HasChosenElement     { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Sin elemento por defecto — el jugador DEBE elegir en ElementSelector
        HasChosenElement = false;

        if (affinityData == null)
            Debug.LogError("[GameSession] AffinityData no asignado en el Inspector.", this);
    }

    public void SetMainElement(ElementType element)
    {
        MainElement      = element;
        HasChosenElement = true;
        Debug.Log($"[GameSession] Elemento principal: {element}");
    }

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

    public void ResetSession()
    {
        HasChosenElement = false;
    }
}