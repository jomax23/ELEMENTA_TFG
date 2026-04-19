using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{



    public void Play()
    {
        SceneManager.LoadScene("Scenes/Map1");
    }
    
    public void Info()
    {
        SceneManager.LoadScene("Scenes/Info");
    }
}
