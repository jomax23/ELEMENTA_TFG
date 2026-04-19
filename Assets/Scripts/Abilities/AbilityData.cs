using UnityEngine;

public abstract class AbilityData : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    
    [TextArea]
    public string description;
    
    public ElementType element;

    [Header("UI")]
    public Sprite icon;
    
    [Header("Cooldown")]
    public float cooldown = 1f;

    // Más adelante:
    // daño, coste, animación, VFX, etc.

    public abstract void Activate(GameObject owner);
}