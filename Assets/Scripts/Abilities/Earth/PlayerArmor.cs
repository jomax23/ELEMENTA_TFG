using System;
using UnityEngine;

public class PlayerArmor : MonoBehaviour
{
    [Header("Material Swap")]
    [SerializeField] private Renderer characterRenderer;
    [SerializeField] private Material armorMaterial;

    // Runtime
    private IArmorUser armorUser;  // ← Interface en vez de PlayerMovement
    private Material[] originalMaterials;

    public event Action OnArmorBroken;
    public bool IsActive { get; private set; }

    private float maxAbsorption;
    private float remainingAbsorption;

    private void Awake()
    {
        // ← Busca cualquier componente que implemente IArmorUser
        armorUser = GetComponent<IArmorUser>();
        
        if (armorUser == null)
            Debug.LogWarning("[PlayerArmor] No se encontró IArmorUser en este GameObject.", this);

        if (characterRenderer != null)
            originalMaterials = characterRenderer.sharedMaterials;
    }

    public void Activate(float absorptionAmount, float speedMultiplier)
    {
        if (IsActive) return;

        IsActive            = true;
        maxAbsorption       = absorptionAmount;
        remainingAbsorption = absorptionAmount;

        ApplyArmorMaterial();
        armorUser?.SetArmorSpeedMultiplier(speedMultiplier);  // ← Usar interface
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        Break();
    }

    public float AbsorbDamage(float incomingDamage)
    {
        if (!IsActive) return incomingDamage;

        float reducedDamage  = incomingDamage * 0.5f;
        remainingAbsorption -= reducedDamage;

        if (remainingAbsorption <= 0f)
        {
            Break();
            return Mathf.Abs(remainingAbsorption);
        }

        return reducedDamage;
    }

    private void ApplyArmorMaterial()
    {
        if (characterRenderer == null || armorMaterial == null) return;

        int count = characterRenderer.sharedMaterials.Length;
        var matArray = new Material[count];
        for (int i = 0; i < count; i++) matArray[i] = armorMaterial;
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
        armorUser?.SetArmorSpeedMultiplier(1f);  // ← Usar interface
        OnArmorBroken?.Invoke();
    }

    private void OnDestroy() => OnArmorBroken = null;
}