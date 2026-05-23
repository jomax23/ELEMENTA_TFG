using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }

    [Header("Match Timer")]
    [SerializeField] private float matchDuration = 300f;
    [SerializeField] private TextMeshProUGUI timerText; // <-- NUEVO

    [Header("Master Control")]
    [SerializeField] private float triggerAtRemainingTime = 60f;
    [SerializeField] private float masterControlDuration = 30f;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private InputActionReference masterControlAction;

    [Header("Slider Colors")]
    [SerializeField] private Color chargingColor = Color.blue;
    [SerializeField] private Color availableColor = Color.yellow;

    public bool IsMasterControlActive { get; private set; }
    public bool IsAvailable { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsMatchOver { get; private set; }

    public event Action OnMasterControlStart;
    public event Action OnMasterControlEnd;
    public event Action OnMatchEnd;

    private enum MCState { Charging, Available, Active, Expired }
    private MCState currentState = MCState.Charging;
    private float activeTimer;
    private Image fillImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        TimeRemaining = matchDuration;

        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
            loadingSlider.interactable = false;
            fillImage = loadingSlider.fillRect?.GetComponent<Image>();
            UpdateFillColor();
        }

        UpdateTimerText(); // <-- Inicializar
    }

    private void OnEnable() => masterControlAction?.action.Enable();
    private void OnDisable() => masterControlAction?.action.Disable();

    private void Update()
    {
        if (IsMatchOver) return;

        TimeRemaining -= Time.deltaTime;
        UpdateTimerText(); // <-- Actualizar cada frame

        switch (currentState)
        {
            case MCState.Charging:
                float chargeTime = matchDuration - triggerAtRemainingTime;
                float elapsed = matchDuration - TimeRemaining;
                loadingSlider.value = Mathf.Clamp01(elapsed / chargeTime);

                if (TimeRemaining <= triggerAtRemainingTime)
                {
                    currentState = MCState.Available;
                    IsAvailable = true;
                    UpdateFillColor();
                }
                break;

            case MCState.Available:
                loadingSlider.value = 1f;
                if (masterControlAction?.action.WasPressedThisFrame() == true)
                {
                    currentState = MCState.Active;
                    IsMasterControlActive = true;
                    IsAvailable = false;
                    activeTimer = masterControlDuration;
                    OnMasterControlStart?.Invoke();
                }
                break;

            case MCState.Active:
                activeTimer -= Time.deltaTime;
                loadingSlider.value = Mathf.Clamp01(activeTimer / masterControlDuration);

                if (activeTimer <= 0)
                {
                    currentState = MCState.Expired;
                    IsMasterControlActive = false;
                    loadingSlider.value = 0f;
                    UpdateFillColor();
                    OnMasterControlEnd?.Invoke();
                }
                break;

            case MCState.Expired:
                loadingSlider.value = 0f;
                break;
        }

        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            IsMatchOver = true;
            OnMatchEnd?.Invoke();
        }
    }

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
        bool isChargingOrExpired = currentState == MCState.Charging || currentState == MCState.Expired;
        fillImage.color = isChargingOrExpired ? chargingColor : availableColor;
    }

    public bool ShouldBypassAffinity() => IsMasterControlActive;
}