using UnityEngine;

public class UIPositioner : MonoBehaviour
{
    public enum ScreenCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public ScreenCorner corner;
    public Vector2 margin;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (!rectTransform) return;

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        Vector2 pivot = Vector2.zero;

        switch (corner)
        {
            case ScreenCorner.TopLeft:
                anchorMin = new Vector2(0, 1);
                anchorMax = new Vector2(0, 1);
                pivot = new Vector2(0, 1);
                break;
            case ScreenCorner.TopRight:
                anchorMin = new Vector2(1, 1);
                anchorMax = new Vector2(1, 1);
                pivot = new Vector2(1, 1);
                break;
            case ScreenCorner.BottomLeft:
                anchorMin = new Vector2(0, 0);
                anchorMax = new Vector2(0, 0);
                pivot = new Vector2(0, 0);
                break;
            case ScreenCorner.BottomRight:
                anchorMin = new Vector2(1, 0);
                anchorMax = new Vector2(1, 0);
                pivot = new Vector2(1, 0);
                break;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;

        Vector2 offset = new Vector2(
            margin.x * (corner == ScreenCorner.TopLeft || corner == ScreenCorner.BottomLeft ? 1 : -1),
            margin.y * (corner == ScreenCorner.TopLeft || corner == ScreenCorner.TopRight ? -1 : 1)
        );

        rectTransform.anchoredPosition = offset;
    }

    void OnRectTransformDimensionsChange()
    {
        UpdatePosition();
    }
}