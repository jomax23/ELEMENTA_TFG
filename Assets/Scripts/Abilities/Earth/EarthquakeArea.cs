using UnityEngine;

/// <summary>
/// Area effect that continuously stuns and damages targets within its trigger volume.
/// Stats are initialized and scaled by the elemental affinity efficiency multiplier.
/// </summary>
public class EarthquakeArea : MonoBehaviour
{
    private LayerMask targetLayers;
    private float timer;
    private bool initialized;
    private float actualStunDuration;
    private float actualDamagePerSecond;
    private float totalDuration;

    /// <summary>
    /// Initializes the area with target layers and scales effects based on affinity efficiency.
    /// </summary>
    /// <param name="layers">The layer mask of valid targets.</param>
    /// <param name="efficiency">Affinity multiplier (0–1). Scales damage per second and stun duration.</param>
    public void Initialize(LayerMask layers, float scaledDuration, float scaledStun, float scaledDps)
    {
        targetLayers = layers;
        totalDuration = scaledDuration;
        actualStunDuration = scaledStun;
        actualDamagePerSecond = scaledDps;
        initialized = true;
        timer = totalDuration;
    }

    private void Update()
    {
        if (!initialized) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!initialized) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.GetComponent<IAbilityTarget>() is IAbilityTarget target)
        {
            target.ApplyStun(actualStunDuration);
            target.ApplyDamage(actualDamagePerSecond * Time.deltaTime);
        }
    }
}