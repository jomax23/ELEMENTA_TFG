using UnityEngine;

public class EarthquakeArea : MonoBehaviour
{
    [Header("Duration")]
    [SerializeField] private float duration = 3f;

    [Header("Effects")]
    [SerializeField] private float stunDuration    = 1f;
    [SerializeField] private float damagePerSecond = 5f;

    private LayerMask targetLayers;
    private float timer;
    private bool  initialized;

    public void Initialize(LayerMask layers)
    {
        targetLayers = layers;
        initialized  = true;
        timer        = duration;
    }

    private void Update()
    {
        if (!initialized) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!initialized) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null) return;

        target.ApplyStun(stunDuration);
        target.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}