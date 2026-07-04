using System;
using UnityEngine;

// Handles the rock-body armor mechanic: absorbs damage, slows the player, and swaps the material.
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
        // Find any component implementing IArmorUser to apply the speed penalty
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

    // Activates the armor, setting its absorption pool and applying the speed penalty.
    public void Activate(float absorptionAmount, float speedMultiplier)
    {
        if (IsActive) return;

        IsActive = true;
        maxAbsorption = absorptionAmount;
        remainingAbsorption = absorptionAmount;

        ApplyArmorMaterial();
        armorUser?.SetArmorSpeedMultiplier(speedMultiplier);
    }

    // Manually deactivates the armor before it is fully depleted.
    public void Deactivate()
    {
        if (!IsActive) return;
        Break();
    }

    // Calculates damage reduction. Returns the remaining damage to be applied to health.
    public float AbsorbDamage(float incomingDamage)
    {
        if (!IsActive) return incomingDamage;

        // Armor reduces incoming damage by 50% and drains the absorption pool
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

    // Replaces all character materials with the solid armor material
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

    // Cleans up state, restores speed, and reverts the visual material
    private void Break()
    {
        IsActive = false;
        RestoreOriginalMaterials();
        armorUser?.SetArmorSpeedMultiplier(1f); 
        OnArmorBroken?.Invoke();
    }

    private void OnDestroy()
    {
        // Prevent memory leaks from lingering event subscriptions
        OnArmorBroken = null;
    }
}