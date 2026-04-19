using UnityEngine;

public class EarthquakeArea : MonoBehaviour
{
    [Header("Duration")]
    [SerializeField] private float duration = 3f;

    [Header("Effects")]
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float damagePerSecond = 5f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    private float timer;

    private void Start()
    {
        timer = duration;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null)
            return;

        // Stun continuo (refresca duración)
        target.ApplyStun(stunDuration);

        // Daño en el tiempo
        target.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}