using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraGlideController : MonoBehaviour
{
    public List<Transform> cameraPositions = new List<Transform>();
    public float moveSpeed = 2f;
    public Button uiButton;
    public Transform followTarget;
    public int followTargetIndex = -1;

    private int currentIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        // Try to find a button if not assigned
        if (uiButton == null)
        {
            uiButton = FindObjectOfType<Button>(); 
        }

        if (uiButton != null)
        {
            uiButton.onClick.AddListener(() => MoveToNextPosition());
        }
        else
        {
            Debug.LogError("UI Button is not assigned in the Inspector and couldn't be found automatically.");
        }
    }

    void Update()
    {
        // Only allow 'C' key for movement
        if (Input.GetKeyDown(KeyCode.C))
        {
            MoveToNextPosition();
        }

        // Follow target if applicable
        if (followTarget != null && currentIndex == followTargetIndex)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }

    void MoveToNextPosition()
    {
        Debug.Log("MoveToNextPosition() called!");

        if (!isMoving && cameraPositions.Count > 0)
        {
            currentIndex = (currentIndex + 1) % cameraPositions.Count;
            StartCoroutine(SmoothTransition(cameraPositions[currentIndex]));
        }
        else
        {
            Debug.Log("MoveToNextPosition() not triggered: isMoving=" + isMoving + ", cameraPositions.Count=" + cameraPositions.Count);
        }
    }

    IEnumerator SmoothTransition(Transform targetPosition)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        float duration = 1f / moveSpeed;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition.position, elapsedTime / duration);
            transform.rotation = Quaternion.Lerp(startRotation, targetPosition.rotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;
        isMoving = false;
    }
}
