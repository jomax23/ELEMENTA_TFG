using UnityEngine;
using System.Collections;

// Utility component that allows non-MonoBehaviours (like ScriptableObjects) 
// to run Coroutines. ScriptableObjects can't call StartCoroutine() directly, 
// so they delegate to this runner attached to a GameObject.
public class AbilityCoroutineRunner : MonoBehaviour
{
    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}