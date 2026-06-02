using UnityEngine;
using System.Collections;

/// <summary>
/// A lightweight utility component used to execute Coroutines for non-MonoBehaviour classes.
/// Since ScriptableObjects (like AbilityData) or plain C# classes cannot call StartCoroutine(),
/// they can reference an instance of this runner attached to a GameObject to handle asynchronous operations.
/// </summary>
public class AbilityCoroutineRunner : MonoBehaviour
{
    /// <summary>
    /// Starts the provided coroutine routine.
    /// </summary>
    /// <param name="routine">The IEnumerator routine to execute.</param>
    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}