using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// In-game HUD for player abilities.
/// Manages ability slot icons, element icons, and real-time cooldown displays.
/// </summary>
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

    private readonly Color normalColor = Color.white;
    private readonly Color cooldownColor = new Color(1f, 1f, 1f, 0.4f);

    private readonly Dictionary<int, float> activeCooldowns = new();

    // Pre-allocated arrays to prevent GC allocations in Update()
    private readonly int[] tempKeys = new int[4];
    private readonly int[] toRemove = new int[4];

    /// <summary>
    /// Assigns icons to the 4 ability slots.
    /// </summary>
    public void SetAbilities(AbilityData ability1, AbilityData ability2, AbilityData ability3, AbilityData ability4)
    {
        SetSlot(0, ability1);
        SetSlot(1, ability2);
        SetSlot(2, ability3);
        SetSlot(3, ability4);
    }

    /// <summary>
    /// Configures a specific slot with an ability icon and resets its cooldown visual.
    /// </summary>
    private void SetSlot(int index, AbilityData ability)
    {
        if (index < 0 || index >= abilitySlots.Count) return;
        Image slot = abilitySlots[index];

        if (ability == null || ability.icon == null)
        {
            slot.enabled = false;
            return;
        }

        slot.enabled = true;
        slot.sprite = ability.icon;
        slot.color = normalColor;

        // Hide cooldown text when a new ability is set
        if (index < cooldownTexts.Count && cooldownTexts[index] != null)
        {
            cooldownTexts[index].text = "";
            cooldownTexts[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the visual cooldown state for a specific slot.
    /// </summary>
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

    /// <summary>
    /// Initializes the cooldown duration for a slot.
    /// </summary>
    public void StartCooldown(int slotIndex, float duration)
    {
        if (activeCooldowns.ContainsKey(slotIndex))
        {
            activeCooldowns[slotIndex] = duration;
        }
    }

    /// <summary>
    /// Updates the visual state of a slot based on the real remaining cooldown time.
    /// </summary>
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

    /// <summary>
    /// Toggles the visibility of the cooldown text for a specific slot.
    /// </summary>
    private void ShowCooldownText(int index, bool show)
    {
        if (index < cooldownTexts.Count && cooldownTexts[index] != null)
        {
            cooldownTexts[index].gameObject.SetActive(show);
            if (!show) cooldownTexts[index].text = "";
        }
    }

    /// <summary>
    /// Ticks down internal cooldown timers and updates the UI.
    /// Optimized: Uses pre-allocated arrays to avoid List GC allocations every frame.
    /// </summary>
    private void Update()
    {
        if (activeCooldowns.Count == 0) return;

        // Cache keys to avoid modifying collection during iteration
        int keyCount = 0;
        foreach (var key in activeCooldowns.Keys)
        {
            tempKeys[keyCount++] = key;
        }

        int removeCount = 0;
        for (int i = 0; i < keyCount; i++)
        {
            int slot = tempKeys[i];
            if (!activeCooldowns.TryGetValue(slot, out float remaining)) continue;

            remaining -= Time.deltaTime;

            if (remaining <= 0f)
            {
                toRemove[removeCount++] = slot;
                UpdateSlotCooldown(slot, 0f);
            }
            else
            {
                activeCooldowns[slot] = remaining;
                
                // Update text directly for performance
                if (slot < cooldownTexts.Count && cooldownTexts[slot] != null)
                {
                    cooldownTexts[slot].text = $"{Mathf.CeilToInt(remaining)}";
                }
            }
        }

        // Clean up finished cooldowns
        for (int i = 0; i < removeCount; i++)
        {
            activeCooldowns.Remove(toRemove[i]);
        }
    }

    /// <summary>
    /// Updates the main element icon based on the current element type.
    /// </summary>
    public void SetElement(ElementType element)
    {
        if (elementIcon == null) return;

        elementIcon.sprite = element switch
        {
            ElementType.Fire => fireElementIcon,
            ElementType.Water => waterElementIcon,
            ElementType.Earth => earthElementIcon,
            ElementType.Air => airElementIcon,
            _ => elementIcon.sprite
        };
    }
}