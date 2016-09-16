using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadNewSceneName(string _sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_sceneName);
    }

    public void LoadNewSceneNumber(int _sceneNumber)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_sceneNumber);
    }

    public void LoadNextScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadCurrentScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
