using UnityEngine;
using UnityEngine.UI;
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
    public float fpvDistance = 1.5f;
    public float fpvVerticalOffset = 2f;
    public float orbitAcceleration = 90f;

    private int currentIndex = 0;
    private bool isMoving = false;
    private float currentOrbitAngle = 0f;
    private float targetOrbitAngle = 0f;
    private bool orbitRotating = false;

    void Start()
    {
        if (cameraPositions.Count > 0)
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

        UpdateButtonVisibility();

        if (povSwitchButton != null)
            povSwitchButton.onClick.AddListener(SwitchPOV);
        if (cwRotateButton != null)
            cwRotateButton.onClick.AddListener(() => { if (currentIndex == followTargetIndex && !orbitRotating) StartCoroutine(OrbitRotate(90f)); });
        if (ccwRotateButton != null)
            ccwRotateButton.onClick.AddListener(() => { if (currentIndex == followTargetIndex && !orbitRotating) StartCoroutine(OrbitRotate(-90f)); });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) SwitchPOV();
        if (currentIndex == followTargetIndex && player != null)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !orbitRotating) StartCoroutine(OrbitRotate(90f));
            if (Input.GetKeyDown(KeyCode.E) && !orbitRotating) StartCoroutine(OrbitRotate(-90f));
            
            currentOrbitAngle = Mathf.MoveTowardsAngle(currentOrbitAngle, targetOrbitAngle, orbitAcceleration * Time.deltaTime);
            UpdateCameraPosition();
        }
    }

    void UpdateCameraPosition()
    {
        Vector3 desiredPos = player.position + new Vector3(
            fpvDistance * Mathf.Sin(currentOrbitAngle * Mathf.Deg2Rad),
            fpvVerticalOffset,
            fpvDistance * Mathf.Cos(currentOrbitAngle * Mathf.Deg2Rad)
        );
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * moveSpeed);
        Quaternion desiredRot = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * moveSpeed);
    }

    public void SwitchPOV()
    {
        if (!isMoving && cameraPositions.Count > 0)
        {
            currentIndex = (currentIndex + 1) % cameraPositions.Count;
            UpdateButtonVisibility();
            StartCoroutine(SmoothTransition(cameraPositions[currentIndex]));
        }
    }

    IEnumerator SmoothTransition(Transform targetTransform)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;
        float duration = 1f / moveSpeed;

        Vector3 endPos = currentIndex == followTargetIndex && player != null ? 
            player.position + new Vector3(
                fpvDistance * Mathf.Sin(targetOrbitAngle * Mathf.Deg2Rad),
                fpvVerticalOffset,
                fpvDistance * Mathf.Cos(targetOrbitAngle * Mathf.Deg2Rad)
            ) : 
            targetTransform.position;

        Quaternion endRot = currentIndex == followTargetIndex && player != null ? 
            Quaternion.LookRotation(player.position - endPos, Vector3.up) : 
            targetTransform.rotation;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;
        isMoving = false;
    }

    IEnumerator OrbitRotate(float angleDelta)
    {
        orbitRotating = true;
        targetOrbitAngle = Mathf.Round((targetOrbitAngle + angleDelta) / 90f) * 90f;
        yield return new WaitForSeconds(0.2f);
        orbitRotating = false;
    }

    private void UpdateButtonVisibility()
    {
        bool isFPV = currentIndex == followTargetIndex;
        if (cwRotateButton != null) cwRotateButton.gameObject.SetActive(isFPV);
        if (ccwRotateButton != null) ccwRotateButton.gameObject.SetActive(isFPV);
    }
}