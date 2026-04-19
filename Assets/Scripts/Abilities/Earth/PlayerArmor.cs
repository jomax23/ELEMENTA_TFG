using System;
using UnityEngine;

public class PlayerArmor : MonoBehaviour
{
    [Header("Armor Mesh")]
    [SerializeField] private GameObject armorMesh;
    
    private PlayerMovement movement;

    
    /// <summary>Notifica cuando la armadura se rompe.</summary>
    public event Action OnArmorBroken;

    public bool IsActive { get; private set; }

    private float maxAbsorption;
    private float remainingAbsorption;

    
    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }
    
    
    /// <summary>
    /// Activa la armadura. El mesh se habilita y los efectos
    /// quedan interceptados hasta agotar la absorción.
    /// </summary>
    public void Activate(float absorptionAmount, float speedMultiplier)
    {
        if (IsActive) return;

        IsActive            = true;
        maxAbsorption       = absorptionAmount;
        remainingAbsorption = absorptionAmount;

        if (armorMesh != null)
            armorMesh.SetActive(true);
        
        movement?.SetArmorSpeedMultiplier(speedMultiplier);
    }
    public void Deactivate()
    {
        if (!IsActive) return;

        Break(); // reutilizamos lógica existente
    }
    /// <summary>
    /// Procesa el daño entrante. Devuelve el daño real que debe
    /// aplicarse al jugador tras la reducción de la armadura.
    /// </summary>
    public float AbsorbDamage(float incomingDamage)
    {
        if (!IsActive) return incomingDamage;

        float reducedDamage = incomingDamage * 0.5f;
        remainingAbsorption -= reducedDamage;

        if (remainingAbsorption <= 0f)
        {
            Break();
            // El exceso de daño (si lo hay) se aplica igualmente
            return Mathf.Abs(remainingAbsorption);
        }

        return reducedDamage;
    }

    private void Break()
    {
        IsActive = false;

        if (armorMesh != null)
            armorMesh.SetActive(false);

        movement?.SetArmorSpeedMultiplier(1f);

        OnArmorBroken?.Invoke();
    }

    private void OnDestroy()
    {
        OnArmorBroken = null;
    }
}