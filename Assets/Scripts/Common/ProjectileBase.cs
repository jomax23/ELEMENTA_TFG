using UnityEngine;

/// <summary>
/// Clase base para todos los proyectiles de ELEMENTA.
///
/// POR QUÉ RAYCAST EN VEZ DE OnTriggerEnter PARA OBSTÁCULOS:
///   OnTriggerEnter NO se dispara entre un trigger (proyectil) y un collider
///   sólido estático sin Rigidbody. Unity requiere al menos un Rigidbody en la
///   pareja para generar callbacks de física. Los muros del escenario son
///   colliders estáticos — sin Rigidbody, sin isTrigger — así que los triggers
///   del proyectil los ignoran por completo.
///
///   Raycast sí funciona con cualquier collider independientemente de si tiene
///   Rigidbody o no, y además evita el tunneling en proyectiles rápidos.
///
///   OnTriggerEnter se mantiene SOLO para detectar targets (jugador/enemigo),
///   donde CharacterController garantiza los callbacks.
///
/// SETUP EN LOS PREFABS:
///   Campo "Obstacle Layers" → asigna las capas de tu geometría de escenario
///   (ej: "Default", "Ground", "Wall"). Sin esto el proyectil no detecta nada.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Obstacle Collision")]
    [Tooltip("Capas que bloquean el proyectil (paredes, suelo, plataformas...).\n" +
             "No requieren Rigidbody ni isTrigger — el Raycast los detecta igual.")]
    [SerializeField] private LayerMask obstacleLayers;

    /// <summary>Asignar en Initialize() de la subclase.</summary>
    protected LayerMask targetLayers;

    /// <summary>
    /// Acceso de solo lectura a obstacleLayers para subclases que necesiten
    /// hacer sus propios Raycasts (ej: RayoMortalProjectile en Initialize).
    /// </summary>
    protected LayerMask ObstacleLayers => obstacleLayers;

    // ─────────────────────────────────────────────────────────────────────────
    // MOVIMIENTO + DETECCIÓN DE OBSTÁCULOS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mueve el proyectil y comprueba obstáculos con Raycast antes de moverse.
    ///
    /// Retorna <c>true</c> si el movimiento fue limpio.
    /// Retorna <c>false</c> si impactó con un obstáculo (el objeto puede haberse
    /// destruido — no lo referencies tras recibir false).
    /// </summary>
    protected bool TryMove(Vector3 direction, float speed)
    {
        float step      = speed * Time.deltaTime;
        // Margen adelantado: evita que proyectiles rápidos entren
        // un fotograma dentro de la geometría antes de detectarla.
        float lookAhead = step + 0.15f;

        if (Physics.Raycast(transform.position, direction, lookAhead, obstacleLayers))
        {
            OnObstacleHit();
            return false;
        }

        transform.position += direction * step;
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TARGET DETECTION (triggers siguen funcionando con CharacterController)
    // ─────────────────────────────────────────────────────────────────────────

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (IsInMask(other.gameObject.layer, targetLayers))
            OnTargetHit(other);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEMPLATE METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado cuando el raycast detecta un obstáculo.
    /// Override para añadir VFX/SFX de impacto antes del Destroy.
    /// </summary>
    protected virtual void OnObstacleHit()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Llamado cuando el trigger solapa con una capa válida de target.
    /// </summary>
    protected abstract void OnTargetHit(Collider target);

    // ─────────────────────────────────────────────────────────────────────────

    protected static bool IsInMask(int layer, LayerMask mask) =>
        (mask.value & (1 << layer)) != 0;
}