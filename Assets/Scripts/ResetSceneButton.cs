using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetSceneButton : MonoBehaviour
{
    public Button resetButton;

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetScene);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
