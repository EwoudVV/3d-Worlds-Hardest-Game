using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute { }

[System.Serializable]
public class ObjectAction
{
    public enum ActionType { Move, Rotate, Wait }
    public ActionType type;

    [Header("Movement Settings")]
    public GameObject moveTarget;

    [Header("Rotation Settings")]
    public Vector3 rotationDegrees;

    [Header("Timing Control")]
    [Tooltip("Check to use speed, uncheck to use duration")]
    public bool useSpeed;
    [Tooltip("Units/sec for movement, deg/sec for rotation")]
    public float speed;
    [Tooltip("Duration in seconds")]
    public float duration;

    [SerializeField, ReadOnly] 
    public float calculatedDuration;
}

[System.Serializable]
public class ObjectEntry
{
    public GameObject targetObject;
    public List<ObjectAction> actions = new List<ObjectAction>();
}

public class Controller : MonoBehaviour
{
    public List<ObjectEntry> objectEntries = new List<ObjectEntry>();
    private List<Coroutine> allCoroutines = new List<Coroutine>();

    void OnValidate()
    {
        foreach (var entry in objectEntries)
        {
            if (entry.targetObject == null) continue;

            foreach (var action in entry.actions)
            {
                UpdateCalculations(entry.targetObject, action);
            }
        }
    }

    void UpdateCalculations(GameObject obj, ObjectAction action)
    {
        switch (action.type)
        {
            case ObjectAction.ActionType.Move when action.moveTarget != null:
                float distance = Vector3.Distance(
                    obj.transform.position,
                    action.moveTarget.transform.position
                );
                action.calculatedDuration = action.useSpeed ? 
                    distance / action.speed : 
                    action.duration;
                break;

            case ObjectAction.ActionType.Rotate:
                float maxRotation = Mathf.Max(
                    Mathf.Abs(action.rotationDegrees.x),
                    Mathf.Abs(action.rotationDegrees.y),
                    Mathf.Abs(action.rotationDegrees.z)
                );
                action.calculatedDuration = action.useSpeed ? 
                    maxRotation / action.speed : 
                    action.duration;
                break;

            case ObjectAction.ActionType.Wait:
                action.calculatedDuration = action.duration;
                break;
        }
    }

    void Start()
    {
        foreach (var entry in objectEntries)
        {
            if (entry.targetObject != null)
            {
                allCoroutines.Add(StartCoroutine(ProcessEntry(entry)));
            }
        }
    }

    IEnumerator ProcessEntry(ObjectEntry entry)
    {
        // Sequential execution within entry
        foreach (var action in entry.actions)
        {
            yield return ExecuteAction(entry.targetObject, action);
        }
    }

    IEnumerator ExecuteAction(GameObject obj, ObjectAction action)
    {
        float duration = action.useSpeed ? action.calculatedDuration : action.duration;

        switch (action.type)
        {
            case ObjectAction.ActionType.Move:
                if (action.moveTarget != null)
                {
                    yield return MoveAction(obj, action);
                }
                break;

            case ObjectAction.ActionType.Rotate:
                yield return RotateAction(obj, action);
                break;

            case ObjectAction.ActionType.Wait:
                yield return new WaitForSeconds(duration);
                break;
        }
    }

    IEnumerator MoveAction(GameObject obj, ObjectAction action)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = action.moveTarget.transform.position;
        float duration = action.useSpeed ? 
            Vector3.Distance(startPos, targetPos) / action.speed : 
            action.duration;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(
                startPos, 
                targetPos, 
                elapsed / duration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = targetPos;
    }

    IEnumerator RotateAction(GameObject obj, ObjectAction action)
    {
        Vector3 startRot = obj.transform.eulerAngles;
        Vector3 targetRot = startRot + action.rotationDegrees;
        float duration = action.useSpeed ? 
            Mathf.Max(
                Mathf.Abs(action.rotationDegrees.x),
                Mathf.Abs(action.rotationDegrees.y),
                Mathf.Abs(action.rotationDegrees.z)
            ) / action.speed : 
            action.duration;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            obj.transform.eulerAngles = new Vector3(
                Mathf.LerpAngle(startRot.x, targetRot.x, elapsed / duration),
                Mathf.LerpAngle(startRot.y, targetRot.y, elapsed / duration),
                Mathf.LerpAngle(startRot.z, targetRot.z, elapsed / duration)
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.eulerAngles = targetRot;
    }
}