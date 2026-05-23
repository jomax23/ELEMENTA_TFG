using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}
