using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreenButtons : MonoBehaviour
{
    [SerializeField] public string sceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void startGameButton()
    {
        SceneManager.LoadScene(sceneName);
    }
}
