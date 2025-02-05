using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Button loadSceneButton;
    public string sceneName;

    void Start()
    {
        if (loadSceneButton != null)
        {
            loadSceneButton.onClick.AddListener(LoadScene);
        }
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is not set!");
        }
    }
}
