using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CameraGlideController : MonoBehaviour
{
    public List<Transform> cameraPositions = new List<Transform>();
    public float moveSpeed = 2f;
    public Button povSwitchButton;
    public Button cwRotateButton;
    public Button ccwRotateButton;
    public Transform followTarget;
    public int followTargetIndex = 0;
    public Transform player;
    public float rotationDuration = 0.5f;
    public float rotationRepeatDelay = 0.2f;

    private int currentIndex = 0;
    private bool isMoving = false;
    private bool isRotating = false;
    private Vector3 followOffset;

    void Start()
    {
        if (cameraPositions != null && cameraPositions.Count > 0)
        {
            currentIndex = 0;
            transform.position = cameraPositions[0].position;
            transform.rotation = cameraPositions[0].rotation;
        }
        else if (followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
        if (player != null)
            followOffset = transform.position - player.position;
        if (povSwitchButton != null)
            povSwitchButton.onClick.AddListener(SwitchPOV);
        if (cwRotateButton != null)
            cwRotateButton.onClick.AddListener(() => StartCoroutine(HandleRotateCW()));
        if (ccwRotateButton != null)
            ccwRotateButton.onClick.AddListener(() => StartCoroutine(HandleRotateCCW()));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            SwitchPOV();
        HandleKeyboardRotationInput();
        if (!isMoving && !isRotating && player != null && currentIndex == followTargetIndex)
            UpdateFollowPosition();
    }

    void HandleKeyboardRotationInput()
    {
        if (Input.GetKey(KeyCode.Q))
            StartCoroutine(ContinuousRotate(-90f));
        if (Input.GetKey(KeyCode.E))
            StartCoroutine(ContinuousRotate(90f));
    }

    IEnumerator ContinuousRotate(float angle)
    {
        while (Input.GetKey(angle > 0 ? KeyCode.E : KeyCode.Q))
        {
            yield return StartCoroutine(SingleRotate(angle));
            yield return new WaitForSeconds(rotationRepeatDelay);
        }
    }

    IEnumerator HandleRotateCW()
    {
        do {
            yield return StartCoroutine(SingleRotate(90f));
            yield return new WaitForSeconds(rotationRepeatDelay);
        } while (IsButtonHeld(ccwRotateButton));
    }

    IEnumerator HandleRotateCCW()
    {
        do {
            yield return StartCoroutine(SingleRotate(-90f));
            yield return new WaitForSeconds(rotationRepeatDelay);
        } while (IsButtonHeld(cwRotateButton));
    }

    bool IsButtonHeld(Button button)
    {
        return EventSystem.current.currentSelectedGameObject == button.gameObject && Input.GetMouseButton(0);
    }

    void UpdateFollowPosition()
    {
        Vector3 targetPos = player.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
        Quaternion targetRot = Quaternion.LookRotation(-followOffset, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * moveSpeed);
    }

    public void SwitchPOV() { MoveToNextPosition(); }

    void MoveToNextPosition()
    {
        if (!isMoving && cameraPositions.Count > 0)
        {
            currentIndex = (currentIndex + 1) % cameraPositions.Count;
            StartCoroutine(SmoothTransition(cameraPositions[currentIndex]));
        }
    }

    IEnumerator SmoothTransition(Transform targetPosition)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsedTime = 0f;
        float duration = 1f / moveSpeed;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition.position, elapsedTime / duration);
            transform.rotation = Quaternion.Lerp(startRot, targetPosition.rotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;
        isMoving = false;
        if (player != null && currentIndex == followTargetIndex)
            followOffset = transform.position - player.position;
    }

    IEnumerator SingleRotate(float angle)
    {
        if (currentIndex != followTargetIndex || player == null)
            yield break;
        isRotating = true;
        Vector3 startOffset = followOffset;
        Vector3 targetOffset = Quaternion.Euler(0, angle, 0) * followOffset;
        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            followOffset = Vector3.Lerp(startOffset, targetOffset, elapsed / rotationDuration);
            transform.position = player.position + followOffset;
            transform.rotation = Quaternion.Lerp(Quaternion.LookRotation(-startOffset, Vector3.up), Quaternion.LookRotation(-targetOffset, Vector3.up), elapsed / rotationDuration);
            yield return null;
        }
        followOffset = targetOffset;
        transform.position = player.position + followOffset;
        transform.rotation = Quaternion.LookRotation(-followOffset, Vector3.up);
        isRotating = false;
    }
}
