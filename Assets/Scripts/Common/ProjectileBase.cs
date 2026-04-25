using UnityEngine;

/// <summary>
/// Clase base para todos los proyectiles de ELEMENTA.
///
/// Encapsula la lógica de colisión con obstáculos y targets mediante el
/// patrón Template Method: la subclase sólo implementa OnTargetHit()
/// y opcionalmente OnObstacleHit() si necesita efectos especiales al impactar.
///
/// SETUP EN EL INSPECTOR (en cada prefab de proyectil):
///   - obstacleLayers: asigna las capas que deben bloquear el proyectil
///     (ej: "Default", "Ground", "Wall").
///   - El proyectil se destruye automáticamente al chocar con un obstáculo.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Obstacle Collision")]
    [Tooltip("Capas que bloquean y destruyen el proyectil (paredes, suelo, plataformas, etc.)")]
    [SerializeField] private LayerMask obstacleLayers;

    // Las subclases asignan esto en Initialize()
    protected LayerMask targetLayers;

    // ─────────────────────────────────────────────────────────────────────────
    // COLISIÓN CENTRALIZADA
    // ─────────────────────────────────────────────────────────────────────────

    protected virtual void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;

        // Los obstáculos tienen prioridad sobre los targets.
        // Si el proyectil golpea simultáneamente ambos, no queremos que
        // el daño atraviese la pared.
        if (IsInMask(layer, obstacleLayers))
        {
            OnObstacleHit(other);
            return;
        }

        if (IsInMask(layer, targetLayers))
            OnTargetHit(other);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEMPLATE METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Comportamiento al impactar con un obstáculo.
    /// Por defecto destruye el proyectil; haz override para añadir VFX/SFX.
    /// </summary>
    protected virtual void OnObstacleHit(Collider obstacle)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Comportamiento al impactar con un target válido (enemy/player layer).
    /// Las subclases aplican daño, efectos de estado, etc.
    /// </summary>
    protected abstract void OnTargetHit(Collider target);

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────────────────

    protected static bool IsInMask(int layer, LayerMask mask) =>
        (mask.value & (1 << layer)) != 0;
}