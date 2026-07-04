using UnityEngine;

// Simple physical barrier that blocks paths and self-destructs after a set time.
public class StoneWall : MonoBehaviour
{
    // Receives the pre-scaled lifetime directly from the AbilityData ScriptableObject
    public void Initialize(float scaledLifetime)
    {
        Destroy(gameObject, scaledLifetime);
    }
}