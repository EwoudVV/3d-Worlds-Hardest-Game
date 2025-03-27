using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DisappearButtonController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonAction
    {
        public Button button;
        public KeyCode activationKey;
        public GameObject optionalTarget;
        public float moveDistance;
        public float moveDuration;
        [HideInInspector] public Vector2 startPos;
        [HideInInspector] public bool isMoving;
    }

    public ButtonAction[] buttonActions;
    private bool isLoadingNextScene;

    void Start()
    {
        foreach (var action in buttonActions)
        {
            action.startPos = action.button.GetComponent<RectTransform>().anchoredPosition;
            action.button.onClick.AddListener(() => StartMovement(action));
        }
    }

    void Update()
    {
        foreach (var action in buttonActions)
        {
            if (Input.GetKeyDown(action.activationKey) && !action.isMoving)
            {
                StartMovement(action);
            }
        }
    }

    void StartMovement(ButtonAction action)
    {
        if (action.isMoving) return;
        action.isMoving = true;
        StartCoroutine(AnimateMovement(action));
    }

    IEnumerator AnimateMovement(ButtonAction action)
    {
        RectTransform buttonRect = action.button.GetComponent<RectTransform>();
        Vector2 buttonEnd = action.startPos + Vector2.up * action.moveDistance;

        RectTransform targetRect = null;
        Vector2 targetStart = Vector2.zero;
        Vector2 targetEnd = Vector2.zero;
        
        if (action.optionalTarget != null)
        {
            targetRect = action.optionalTarget.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                targetStart = targetRect.anchoredPosition;
                targetEnd = targetStart + Vector2.up * action.moveDistance;
            }
        }

        float elapsed = 0;
        while (elapsed < action.moveDuration)
        {
            float t = elapsed / action.moveDuration;
            buttonRect.anchoredPosition = Vector2.Lerp(action.startPos, buttonEnd, t);
            
            if (targetRect != null)
            {
                targetRect.anchoredPosition = Vector2.Lerp(targetStart, targetEnd, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        buttonRect.anchoredPosition = buttonEnd;
        action.button.gameObject.SetActive(false);

        if (targetRect != null)
        {
            targetRect.anchoredPosition = targetEnd;
            action.optionalTarget.SetActive(false);
        }

        action.isMoving = false;

        if (CheckAllButtonsCompleted() && !isLoadingNextScene)
        {
            isLoadingNextScene = true;
            StartCoroutine(LoadNextScene());
        }
    }

    bool CheckAllButtonsCompleted()
    {
        foreach (var action in buttonActions)
        {
            if (action.button.gameObject.activeSelf) return false;
        }
        return true;
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}