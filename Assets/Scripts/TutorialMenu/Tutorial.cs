using UnityEngine;
using UnityEngine.SceneManagement;

// Simple scene loader for the tutorial screen.
public class Tutorial : MonoBehaviour
{
    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}