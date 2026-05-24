using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AbilitiesHUD : MonoBehaviour
{
    [Header("Ability Slots")]
    [SerializeField] private List<Image> abilitySlots;
    [SerializeField] private List<TextMeshProUGUI> cooldownTexts;

    [Header("Element Icon")]
    [SerializeField] private Image elementIcon;

    [Header("Element Icons")]
    [SerializeField] private Sprite fireElementIcon;
    [SerializeField] private Sprite waterElementIcon;
    [SerializeField] private Sprite earthElementIcon;
    [SerializeField] private Sprite airElementIcon;

    private Color normalColor = Color.white;
    private Color cooldownColor = new Color(1f, 1f, 1f, 0.4f);
    
    private Dictionary<int, float> activeCooldowns = new();

    public void SetAbilities(AbilityData ability1, AbilityData ability2, AbilityData ability3, AbilityData ability4)
    {
        SetSlot(0, ability1); SetSlot(1, ability2); SetSlot(2, ability3); SetSlot(3, ability4);
    }

    private void SetSlot(int index, AbilityData ability)
    {
        if (index < 0 || index >= abilitySlots.Count) return;
        Image slot = abilitySlots[index];

        if (ability == null || ability.icon == null) { slot.enabled = false; return; }

        slot.enabled = true;
        slot.sprite = ability.icon;
        slot.color = normalColor;
        
        if (index < cooldownTexts.Count && cooldownTexts[index] != null)
        {
            cooldownTexts[index].text = "";
            cooldownTexts[index].gameObject.SetActive(false);
        }
    }

    public void SetCooldown(int slotIndex, bool onCooldown)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Count) return;
        
        abilitySlots[slotIndex].color = onCooldown ? cooldownColor : normalColor;
        
        if (onCooldown)
        {
            activeCooldowns[slotIndex] = 0f;
            ShowCooldownText(slotIndex, true);
        }
        else
        {
            activeCooldowns.Remove(slotIndex);
            ShowCooldownText(slotIndex, false);
        }
    }
    
    public void StartCooldown(int slotIndex, float duration)
    {
        if (activeCooldowns.ContainsKey(slotIndex))
            activeCooldowns[slotIndex] = duration;
    }

    public void UpdateSlotCooldown(int slotIndex, float remainingTime)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Count) return;
        
        bool onCooldown = remainingTime > 0f;
        abilitySlots[slotIndex].color = onCooldown ? cooldownColor : normalColor;
        
        if (slotIndex < cooldownTexts.Count && cooldownTexts[slotIndex] != null)
        {
            if (onCooldown)
            {
                cooldownTexts[slotIndex].text = $"{Mathf.CeilToInt(remainingTime)}";
                cooldownTexts[slotIndex].gameObject.SetActive(true);
            }
            else
            {
                cooldownTexts[slotIndex].text = "";
                cooldownTexts[slotIndex].gameObject.SetActive(false);
            }
        }
    }

    private void ShowCooldownText(int index, bool show)
    {
        if (index < cooldownTexts.Count && cooldownTexts[index] != null)
        {
            cooldownTexts[index].gameObject.SetActive(show);
            if (!show) cooldownTexts[index].text = "";
        }
    }

    private void Update()
    {
        var keys = new List<int>(activeCooldowns.Keys);
        var toRemove = new List<int>();
        
        foreach (int slot in keys)
        {
            if (!activeCooldowns.ContainsKey(slot)) continue;
            
            float remaining = activeCooldowns[slot] - Time.deltaTime;
            
            if (remaining <= 0f)
            {
                toRemove.Add(slot);
                ShowCooldownText(slot, false);
                if (slot < abilitySlots.Count)
                    abilitySlots[slot].color = normalColor;
            }
            else
            {
                activeCooldowns[slot] = remaining;
                if (slot < cooldownTexts.Count && cooldownTexts[slot] != null)
                    cooldownTexts[slot].text = $"{Mathf.CeilToInt(remaining)}";
            }
        }
        
        foreach (int slot in toRemove)
            activeCooldowns.Remove(slot);
    }

    public void SetElement(ElementType element)
    {
        if (elementIcon == null) return;
        elementIcon.sprite = element switch
        {
            ElementType.Fire  => fireElementIcon,
            ElementType.Water => waterElementIcon,
            ElementType.Earth => earthElementIcon,
            ElementType.Air   => airElementIcon,
            _ => elementIcon.sprite
        };
    }
}