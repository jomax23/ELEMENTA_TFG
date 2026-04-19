using UnityEngine;
using System.Collections;

public class AbilityCoroutineRunner : MonoBehaviour
{
    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}