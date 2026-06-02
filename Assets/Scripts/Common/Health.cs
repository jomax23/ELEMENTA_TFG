using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages the health state of an entity, updates a UI slider, and triggers events on death.
/// </summary>
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

    /// <summary>
    /// Reduces health and triggers death if it reaches zero.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        UpdateSlider();

        if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Restores health up to the maximum limit.
    /// </summary>
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