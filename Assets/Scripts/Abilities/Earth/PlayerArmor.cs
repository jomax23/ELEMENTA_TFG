using System;
using UnityEngine;

/// <summary>
/// Manages a temporary armor state for the player.
/// Absorbs a specific amount of damage, applies a movement speed penalty via IArmorUser,
/// and visually swaps the character's material.
/// </summary>
public class PlayerArmor : MonoBehaviour
{
    [Header("Material Swap")]
    [SerializeField] private Renderer characterRenderer;
    [SerializeField] private Material armorMaterial;

    // Runtime state
    private IArmorUser armorUser;
    private Material[] originalMaterials;

    public event Action OnArmorBroken;
    public bool IsActive { get; private set; }

    private float maxAbsorption;
    private float remainingAbsorption;

    private void Awake()
    {
        // Decoupled from PlayerMovement: finds any component implementing IArmorUser
        armorUser = GetComponent<IArmorUser>();
        
        if (armorUser == null)
        {
            Debug.LogWarning("[PlayerArmor] No IArmorUser component found on this GameObject.", this);
        }

        if (characterRenderer != null)
        {
            originalMaterials = characterRenderer.sharedMaterials;
        }
    }

    /// <summary>
    /// Activates the armor, setting its absorption pool and applying the speed penalty.
    /// </summary>
    public void Activate(float absorptionAmount, float speedMultiplier)
    {
        if (IsActive) return;

        IsActive = true;
        maxAbsorption = absorptionAmount;
        remainingAbsorption = absorptionAmount;

        ApplyArmorMaterial();
        armorUser?.SetArmorSpeedMultiplier(speedMultiplier);
    }

    /// <summary>
    /// Manually deactivates the armor before it is fully depleted.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        Break();
    }

    /// <summary>
    /// Calculates damage reduction. Returns the remaining damage to be applied to health.
    /// </summary>
    public float AbsorbDamage(float incomingDamage)
    {
        if (!IsActive) return incomingDamage;

        // Armor reduces incoming damage by 50%
        float reducedDamage = incomingDamage * 0.5f;
        remainingAbsorption -= reducedDamage;

        if (remainingAbsorption <= 0f)
        {
            Break();
            // Return any overflow damage that exceeded the armor's capacity
            return Mathf.Abs(remainingAbsorption); 
        }

        return reducedDamage;
    }

    private void ApplyArmorMaterial()
    {
        if (characterRenderer == null || armorMaterial == null) return;

        int count = characterRenderer.sharedMaterials.Length;
        Material[] matArray = new Material[count];
        for (int i = 0; i < count; i++)
        {
            matArray[i] = armorMaterial;
        }
        characterRenderer.materials = matArray;
    }

    private void RestoreOriginalMaterials()
    {
        if (characterRenderer == null || originalMaterials == null) return;
        characterRenderer.materials = originalMaterials;
    }

    private void Break()
    {
        IsActive = false;
        RestoreOriginalMaterials();
        armorUser?.SetArmorSpeedMultiplier(1f); // Restore normal speed
        OnArmorBroken?.Invoke();
    }

    private void OnDestroy()
    {
        // Prevent memory leaks
        OnArmorBroken = null;
    }
}