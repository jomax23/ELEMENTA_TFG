#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth;

    public Slider slider;

    
    private void Awake()
    {
        health = maxHealth;
        slider.maxValue = maxHealth;
        slider.value = health;
    }
    
    
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        slider.value = health;
        if (health <= 0)
        {
            //Destroy(gameObject);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
    
    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        slider.value = health;
    }
}
