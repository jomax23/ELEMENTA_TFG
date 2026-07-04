using UnityEngine;
using UnityEngine.UI;
using System;

// Simple wrapper for the element selection tabs (Fire, Water, etc.).
// Just handles the click callback to keep the main UI script clean.
public class ElementButtonUIAbilities : MonoBehaviour
{
    private ElementType _element;
    private Action _onClick;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Init(ElementType element, Action onClick)
    {
        _element = element;
        _onClick = onClick;
        
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners(); 
            _button.onClick.AddListener(TriggerClick);
        }
    }

    private void TriggerClick() => _onClick?.Invoke();
}