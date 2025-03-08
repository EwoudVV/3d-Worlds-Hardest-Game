using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CameraGlideController : MonoBehaviour
{
    public List<Transform> cameraPositions = new List<Transform>();
    public float moveSpeed = 2f;
    public float povTransitionTime = 0.5f;
    public Button povSwitchButton;
    public Button cwRotateButton;
    public Button ccwRotateButton;
    public Transform followTarget;
    public int followTargetIndex = 0;
    public Transform player;
    public float fpvDistance = 3f;
    public float fpvVerticalOffset = 2f;
    public float orbitAcceleration = 90f;

    private int currentIndex = 0;
    private bool isMoving = false;
    private float currentOrbitAngle = 0f;
    private float targetOrbitAngle = 0f;
    private bool orbitRotating = false;

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
        if (currentIndex == followTargetIndex)
        {
            currentOrbitAngle = 0f;
            targetOrbitAngle = 0f;
        }
        if (povSwitchButton != null)
            povSwitchButton.onClick.AddListener(SwitchPOV);
        if (cwRotateButton != null)
            cwRotateButton.onClick.AddListener(() => { if (currentIndex == followTargetIndex && !orbitRotating) StartCoroutine(OrbitRotateContinuous(90f)); });
        if (ccwRotateButton != null)
            ccwRotateButton.onClick.AddListener(() => { if (currentIndex == followTargetIndex && !orbitRotating) StartCoroutine(OrbitRotateContinuous(-90f)); });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            SwitchPOV();
        if (currentIndex == followTargetIndex && player != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && !orbitRotating)
                StartCoroutine(OrbitRotateContinuous(90f));
            if (Input.GetKeyDown(KeyCode.Q) && !orbitRotating)
                StartCoroutine(OrbitRotateContinuous(-90f));
            currentOrbitAngle = Mathf.MoveTowardsAngle(currentOrbitAngle, targetOrbitAngle, orbitAcceleration * Time.deltaTime);
            Vector3 desiredPos = player.position + new Vector3(fpvDistance * Mathf.Sin(currentOrbitAngle * Mathf.Deg2Rad), fpvVerticalOffset, fpvDistance * Mathf.Cos(currentOrbitAngle * Mathf.Deg2Rad));
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * moveSpeed);
            Quaternion desiredRot = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * moveSpeed);
        }
    }

    public void SwitchPOV()
    {
        if (!isMoving && cameraPositions.Count > 0)
        {
            currentIndex = (currentIndex + 1) % cameraPositions.Count;
            if (currentIndex == followTargetIndex && player != null)
            {
                currentOrbitAngle = 0f;
                targetOrbitAngle = 0f;
            }
            StartCoroutine(SmoothTransition(cameraPositions[currentIndex]));
        }
    }

    IEnumerator SmoothTransition(Transform targetTransform)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;
        float duration = (currentIndex == followTargetIndex && player != null) ? povTransitionTime : (1f / moveSpeed);
        Vector3 endPos;
        Quaternion endRot;
        if (currentIndex == followTargetIndex && player != null)
        {
            endPos = player.position + new Vector3(fpvDistance * Mathf.Sin(targetOrbitAngle * Mathf.Deg2Rad), fpvVerticalOffset, fpvDistance * Mathf.Cos(targetOrbitAngle * Mathf.Deg2Rad));
            endRot = Quaternion.LookRotation(player.position - endPos, Vector3.up);
        }
        else
        {
            endPos = targetTransform.position;
            endRot = targetTransform.rotation;
        }
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
        if (currentIndex == followTargetIndex && player != null)
            currentOrbitAngle = targetOrbitAngle;
    }

    IEnumerator OrbitRotateContinuous(float angleDelta)
    {
        orbitRotating = true;
        while (Input.GetKey(angleDelta > 0 ? KeyCode.E : KeyCode.Q) ||
               (cwRotateButton != null && EventSystem.current.currentSelectedGameObject == cwRotateButton.gameObject) ||
               (ccwRotateButton != null && EventSystem.current.currentSelectedGameObject == ccwRotateButton.gameObject))
        {
            if (Mathf.Abs(Mathf.DeltaAngle(currentOrbitAngle, targetOrbitAngle)) < 0.1f)
            {
                targetOrbitAngle += angleDelta;
                while (Mathf.Abs(Mathf.DeltaAngle(currentOrbitAngle, targetOrbitAngle)) > 0.1f)
                    yield return null;
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }
        orbitRotating = false;
    }
}
