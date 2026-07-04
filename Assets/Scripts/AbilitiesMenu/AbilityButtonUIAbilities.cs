using UnityEngine;
using UnityEngine.UI;
using System;

// Represents a single ability slot in the UI. 
// Holds the data reference, updates the icon, and fires a callback when clicked.
public class AbilityButtonUIAbilities : MonoBehaviour
{
    [SerializeField] private Image icon;
    
    public AbilityData Ability { get; private set; }
    
    private Action _onClick;
    private Button _button;

    private void Awake()
    {
        // Cache the Button component once to avoid runtime GetComponent allocations
        _button = GetComponent<Button>();
    }

    // Called by the main UI controller to wire up the data and click event
    public void Init(AbilityData ability, Action onClick)
    {
        Ability = ability;
        _onClick = onClick;

        // Set the icon if data exists, otherwise hide the image
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
            // Crucial: clear old listeners to prevent duplicate triggers if this button is recycled
            _button.onClick.RemoveAllListeners(); 
            _button.onClick.AddListener(TriggerClick);
        }
    }

    private void TriggerClick() => _onClick?.Invoke();
}