using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrampaVolcanicaArea : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float activationTime  = 1f;
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float lifetime        = 4f;

    [Header("Spawn Animation")]
    [SerializeField] private float appearDuration = 0.35f;
    [SerializeField] private float spawnDepth     = -1.2f;
    [SerializeField] private float finalScale     = 1f;

    private LayerMask targetLayers;
    private bool initialized;

    private class TargetData
    {
        public float timeInside;
        public bool  isActive;
    }

    private Dictionary<IAbilityTarget, TargetData> targets = new();

    private bool   trapActive;
    private Vector3 startPosition;

    /// <summary>
    /// Llamado desde la ability inmediatamente tras Instantiate (antes de Start).
    /// Awake ya habrá corrido, Start aún no.
    /// </summary>
    public void Initialize(LayerMask layers)
    {
        targetLayers = layers;
        initialized  = true;
    }

    private void Start()
    {
        startPosition = transform.position;

        transform.localScale = Vector3.zero;
        transform.position  += Vector3.up * spawnDepth;

        StartCoroutine(AppearRoutine());
    }

    private IEnumerator AppearRoutine()
    {
        float   time          = 0f;
        Vector3 targetPosition = startPosition;
        Vector3 startScale    = Vector3.zero;
        Vector3 endScale      = Vector3.one * finalScale;

        while (time < appearDuration)
        {
            float t = time / appearDuration;

            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position   = Vector3.Lerp(
                startPosition + Vector3.up * spawnDepth,
                targetPosition,
                t
            );

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
        transform.position   = targetPosition;

        trapActive = true;
        Destroy(gameObject, lifetime);
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
                    data.isActive = true;
            }
            else
            {
                pair.Key.ApplyDamage(damagePerSecond * delta);
            }
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