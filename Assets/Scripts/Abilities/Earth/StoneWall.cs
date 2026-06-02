using UnityEngine;

/// <summary>
/// A simple spatial control obstacle that self-destructs after a set lifetime.
/// </summary>
public class StoneWall : MonoBehaviour
{
    /// <summary>
    /// Initializes the wall with its pre-scaled lifetime from the ScriptableObject.
    /// </summary>
    public void Initialize(float scaledLifetime)
    {
        Destroy(gameObject, scaledLifetime);
    }
}