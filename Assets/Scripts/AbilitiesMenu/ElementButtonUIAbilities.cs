// ElementButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementButtonUIAbilities : MonoBehaviour
{
    private ElementType _element;
    private System.Action _onClick;

    public void Init(ElementType element, System.Action onClick)
    {
        _element = element;
        _onClick = onClick;
        GetComponent<Button>().onClick.AddListener(TriggerClick);
    }

    private void TriggerClick() => _onClick?.Invoke();
    
}