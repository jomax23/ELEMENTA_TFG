#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth;

    public Slider slider;

    public event Action OnDeath;
    
    private void Awake()
    {
        health = maxHealth;
        slider.maxValue = maxHealth;
        slider.value = health;
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        slider.value = health;
        if (health <= 0)
        {
            health = 0;
            slider.value = 0;
            OnDeath?.Invoke();
        }
    }
    
    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        slider.value = health;
    }
}
