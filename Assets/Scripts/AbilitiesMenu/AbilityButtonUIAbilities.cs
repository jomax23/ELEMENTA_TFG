using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI component for an ability selection button in the Abilities Info screen.
/// Displays the ability icon and triggers a registered callback when clicked.
/// </summary>
public class AbilityButtonUIAbilities : MonoBehaviour
{
    [SerializeField] private Image icon;
    
    public AbilityData Ability { get; private set; }
    private Action _onClick;
    private Button _button;

    private void Awake()
    {
        // Cache the Button component to avoid runtime allocation
        _button = GetComponent<Button>();
    }

    /// <summary>
    /// Initializes the button with ability data and a click callback.
    /// </summary>
    public void Init(AbilityData ability, Action onClick)
    {
        Ability = ability;
        _onClick = onClick;

        if (ability != null && ability.icon != null && icon != null)
        {
            icon.sprite = ability.icon;
            icon.enabled = true;
        }
        else if (icon != null)
        {
            icon.enabled = false;
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners(); // Prevent duplicate listeners if Init is called multiple times
            _button.onClick.AddListener(TriggerClick);
        }
    }

    private void TriggerClick() => _onClick?.Invoke();
}