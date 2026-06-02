using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

/// <summary>
/// Singleton that manages the match timer and the "Master Control" mechanic.
/// Master Control is a temporary super-state triggered at the end of the match 
/// that bypasses elemental affinities.
/// </summary>
public class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }

    [Header("Match Timer")]
    [SerializeField] private float matchDuration = 300f;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Master Control")]
    [SerializeField] private float triggerAtRemainingTime = 60f;
    [SerializeField] private float masterControlDuration = 30f;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private InputActionReference masterControlAction;

    [Header("Slider Colors")]
    [SerializeField] private Color chargingColor = Color.blue;
    [SerializeField] private Color availableColor = Color.yellow;

    // ── Public State ───────────────────────────────────────────────────────
    public bool IsMasterControlActive { get; private set; }
    public bool IsAvailable { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsMatchOver { get; private set; }

    public event Action OnMasterControlStart;
    public event Action OnMasterControlEnd;
    public event Action OnMatchEnd;

    // ── Private State ──────────────────────────────────────────────────────
    private enum MCState { Charging, Available, Active, Expired }
    private MCState currentState = MCState.Charging;
    
    private float activeTimer;
    private Image fillImage;

    // =========================================================================
    // INITIALIZATION
    // =========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        
        Instance = this;
        TimeRemaining = matchDuration;

        if (GameSession.Instance != null && GameSession.Instance.ForceMasterControl)
        {
            currentState = MCState.Active;
            IsMasterControlActive = true;
            IsAvailable = false;
            activeTimer = float.MaxValue; // Duración infinita para la demo
        
            if (loadingSlider != null) loadingSlider.value = 1f;
            UpdateFillColor();
            OnMasterControlStart?.Invoke();
        
            Debug.Log("[MatchController] Master Control FORCED ACTIVE for demonstration.");
        }
        // ── FIN NUEVO ──
        else
        {
            if (loadingSlider != null)
            {
                loadingSlider.minValue = 0f;
                loadingSlider.maxValue = 1f;
                loadingSlider.value = 0f;
                loadingSlider.interactable = false;
                fillImage = loadingSlider.fillRect?.GetComponent<Image>();
                UpdateFillColor();
            }
        }

        UpdateTimerText();
    }

    private void OnEnable() => masterControlAction?.action.Enable();
    private void OnDisable() => masterControlAction?.action.Disable();

    // =========================================================================
    // UPDATE LOOP
    // =========================================================================

    private void Update()
    {
        if (IsMatchOver) return;

        TimeRemaining -= Time.deltaTime;
        UpdateTimerText();

        switch (currentState)
        {
            case MCState.Charging:
                HandleChargingState();
                break;

            case MCState.Available:
                HandleAvailableState();
                break;

            case MCState.Active:
                HandleActiveState();
                break;

            case MCState.Expired:
                loadingSlider.value = 0f;
                break;
        }

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsMatchOver = true;
            OnMatchEnd?.Invoke();
        }
    }

    // =========================================================================
    // STATE HANDLERS
    // =========================================================================

    private void HandleChargingState()
    {
        float chargeTime = matchDuration - triggerAtRemainingTime;
        float elapsed = matchDuration - TimeRemaining;
        loadingSlider.value = Mathf.Clamp01(elapsed / chargeTime);

        if (TimeRemaining <= triggerAtRemainingTime)
        {
            currentState = MCState.Available;
            IsAvailable = true;
            UpdateFillColor();
        }
    }

    private void HandleAvailableState()
    {
        loadingSlider.value = 1f;
        
        if (masterControlAction?.action.WasPressedThisFrame() == true)
        {
            currentState = MCState.Active;
            IsMasterControlActive = true;
            IsAvailable = false;
            activeTimer = masterControlDuration;
            OnMasterControlStart?.Invoke();
        }
    }

    private void HandleActiveState()
    {
        activeTimer -= Time.deltaTime;
        loadingSlider.value = Mathf.Clamp01(activeTimer / masterControlDuration);

        if (activeTimer <= 0f)
        {
            currentState = MCState.Expired;
            IsMasterControlActive = false;
            loadingSlider.value = 0f;
            UpdateFillColor();
            OnMasterControlEnd?.Invoke();
        }
    }

    // =========================================================================
    // UI HELPERS
    // =========================================================================

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(TimeRemaining / 60f);
        int seconds = Mathf.FloorToInt(TimeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateFillColor()
    {
        if (fillImage == null) return;
        
        bool isInactive = currentState == MCState.Charging || currentState == MCState.Expired;
        fillImage.color = isInactive ? chargingColor : availableColor;
    }

    // =========================================================================
    // PUBLIC API
    // =========================================================================

    /// <summary>Returns true if Master Control is currently active, bypassing affinities.</summary>
    public bool ShouldBypassAffinity() => IsMasterControlActive;
}