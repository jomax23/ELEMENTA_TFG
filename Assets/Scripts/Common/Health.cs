using UnityEngine;
using UnityEngine.UI;
using System;

// Handles HP tracking, UI slider updates, and death events. 
// Kept simple and decoupled from specific damage types or armor logic.
public class Health : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;

    [Header("UI")]
    [SerializeField] private Slider slider;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        if (slider != null)
        {
            slider.maxValue = maxHealth;
            slider.value = CurrentHealth;
        }
    }

    // Clamps health at 0 and fires the death event if depleted.
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        UpdateSlider();
        
        if (CurrentHealth <= 0f)
            OnDeath?.Invoke();
    }

    // Clamps health at maxHealth.
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (slider != null)
            slider.value = CurrentHealth;
    }
}