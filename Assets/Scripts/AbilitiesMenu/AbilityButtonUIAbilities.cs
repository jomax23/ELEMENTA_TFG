// AbilityButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityButtonUIAbilities : MonoBehaviour
{
    [SerializeField] private Image icon;

    
    public AbilityData Ability { get; private set; }
    private System.Action _onClick;

    public void Init(AbilityData ability, System.Action onClick)
    {
        Ability = ability;
        _onClick = onClick;
        icon.sprite = ability.icon;
        GetComponent<Button>().onClick.AddListener(TriggerClick);
    }

    private void TriggerClick() => _onClick?.Invoke();
    
}