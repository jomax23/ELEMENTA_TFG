using UnityEngine;

// The physical area effect for the Earthquake ability.
// Handles the self-destruct timer and applies continuous effects to targets inside.
public class EarthquakeArea : MonoBehaviour
{
    private LayerMask targetLayers;
    private float timer;
    private bool initialized;
    
    private float actualStunDuration;
    private float actualDamagePerSecond;
    private float totalDuration;

    // Sets up the scaled stats and starts the self-destruct timer
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

    // Applies continuous damage and stun to any valid target inside the trigger
    private void OnTriggerStay(Collider other)
    {
        if (!initialized) return;
        
        // Ignore layers not specified in the mask
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.GetComponent<IAbilityTarget>() is IAbilityTarget target)
        {
            target.ApplyStun(actualStunDuration);
            
            // Multiply DPS by deltaTime to apply damage per frame instead of per second
            target.ApplyDamage(actualDamagePerSecond * Time.deltaTime);
        }
    }
}