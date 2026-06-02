using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Area effect that spawns with an animation, waits for an activation delay, 
/// and then continuously damages targets within its trigger volume.
/// Stats are initialized and scaled by the elemental affinity efficiency multiplier.
/// </summary>
public class TrampaVolcanicaArea : MonoBehaviour
{
    [Header("Spawn Animation")]
    [SerializeField] private float activationTime = 1f;
    [SerializeField] private float appearDuration = 0.35f;
    [SerializeField] private float spawnDepth = -1.2f;
    [SerializeField] private float finalScale = 1f;

    private LayerMask targetLayers;
    private bool initialized;
    
    // Runtime values (injected by AbilityData)
    private float actualDamagePerSecond;
    private float actualLifetime;


    private class TargetData
    {
        public float timeInside;
        public bool isActive;
    }
 
    private readonly Dictionary<IAbilityTarget, TargetData> targets = new();

    private bool trapActive;
    private Vector3 startPosition;

    /// <summary>
    /// Initializes the trap with target layers and scales effects based on affinity efficiency.
    /// </summary>
    /// <param name="layers">The layer mask of valid targets.</param>
    /// <param name="efficiency">Affinity multiplier (0–1). Scales damage per second.</param>
    public void Initialize(LayerMask layers, float scaledDamagePerSecond, float scaledLifetime)
    {
        targetLayers = layers;
        actualDamagePerSecond = scaledDamagePerSecond;
        actualLifetime = scaledLifetime;
        initialized = true;
    }

    private void Start()
    {
        startPosition = transform.position;

        // Start hidden and below ground
        transform.localScale = Vector3.zero;
        transform.position += Vector3.up * spawnDepth;

        StartCoroutine(AppearRoutine());
    }

    private IEnumerator AppearRoutine()
    {
        float time = 0f;
        Vector3 targetPosition = startPosition;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * finalScale;

        while (time < appearDuration)
        {
            float t = time / appearDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position = Vector3.Lerp(startPosition + Vector3.up * spawnDepth, targetPosition, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
        transform.position = targetPosition;
        trapActive = true;
        
        // Use the scaled lifetime passed from the SO
        Destroy(gameObject, actualLifetime);
    }

    private void Update()
    {
        if (!trapActive) return;
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        float delta = Time.deltaTime;

        foreach (var pair in targets)
        {
            TargetData data = pair.Value;

            if (!data.isActive)
            {
                data.timeInside += delta;
                if (data.timeInside >= activationTime)
                {
                    data.isActive = true;
                }
            }
            else
            {
                pair.Key.ApplyDamage(actualDamagePerSecond * delta);
            }
            // Note: Dictionary iteration during modification is safe here 
            // because we only modify the values inside the existing entries.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!trapActive || !initialized) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null || targets.ContainsKey(target)) return;

        targets.Add(target, new TargetData { timeInside = 0f, isActive = false });
    }

    private void OnTriggerExit(Collider other)
    {
        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target != null)
            targets.Remove(target);
    }
}