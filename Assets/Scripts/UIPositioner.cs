using UnityEngine;
using UnityEngine.UI;

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

        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        Vector2 pivot = Vector2.zero;

        switch (corner)
        {
            case ScreenCorner.TopLeft:
                min = new Vector2(0, 1);
                max = new Vector2(0, 1);
                pivot = new Vector2(0, 1);
                break;
            case ScreenCorner.TopRight:
                min = new Vector2(1, 1);
                max = new Vector2(1, 1);
                pivot = new Vector2(1, 1);
                break;
            case ScreenCorner.BottomLeft:
                min = new Vector2(0, 0);
                max = new Vector2(0, 0);
                pivot = new Vector2(0, 0);
                break;
            case ScreenCorner.BottomRight:
                min = new Vector2(1, 0);
                max = new Vector2(1, 0);
                pivot = new Vector2(1, 0);
                break;
        }

        rectTransform.anchorMin = min;
        rectTransform.anchorMax = max;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = new Vector2(margin.x * (corner == ScreenCorner.TopLeft || corner == ScreenCorner.BottomLeft ? 1 : -1), 
                                                   margin.y * (corner == ScreenCorner.TopLeft || corner == ScreenCorner.TopRight ? -1 : 1));
    }

    void OnRectTransformDimensionsChange()
    {
        UpdatePosition();
    }
}