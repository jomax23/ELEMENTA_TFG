/*
using UnityEngine;

public class SoplidoArea : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private float lifetime = 0.1f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    private int directionX;

    public void Initialize(int facingDirection)
    {
        directionX = facingDirection;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null)
            return;

        // Empuje SOLO en X
        target.ApplyImpulse(directionX * pushForce);
    }
}
*/
using UnityEngine;

public class SoplidoArea : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private float lifetime  = 0.15f; // 0.1f puede ser insuficiente para un tick de física

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    private int directionX;
    private bool initialized;

    public void Initialize(int facingDirection)
    {
        directionX  = facingDirection;
        initialized = true;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        // Comprobación de capa usando el operador estándar de LayerMask
        if ((targetLayers & (1 << other.gameObject.layer)) == 0) return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        target?.ApplyImpulse(directionX * pushForce);
    }
}