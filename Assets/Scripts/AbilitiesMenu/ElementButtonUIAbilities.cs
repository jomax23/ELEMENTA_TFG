using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI component for an element selection button in the Abilities Info screen.
/// Triggers a registered callback when clicked.
/// </summary>
public class ElementButtonUIAbilities : MonoBehaviour
{
    private ElementType _element;
    private Action _onClick;
    private Button _button;

    private void Awake()
    {
        // Cache the Button component to avoid runtime allocation
        _button = GetComponent<Button>();
    }

    /// <summary>
    /// Initializes the button with the element type and click callback.
    /// </summary>
    public void Init(ElementType element, Action onClick)
    {
        _element = element;
        _onClick = onClick;
        
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners(); // Prevent duplicate listeners if Init is called multiple times
            _button.onClick.AddListener(TriggerClick);
        }
    }

    private void TriggerClick() => _onClick?.Invoke();
}