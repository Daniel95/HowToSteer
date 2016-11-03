using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadNewSceneName(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void LoadNewSceneNumber(int _sceneNumber)
    {
        SceneManager.LoadScene(_sceneNumber);
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
