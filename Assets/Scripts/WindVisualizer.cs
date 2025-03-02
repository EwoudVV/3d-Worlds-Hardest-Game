using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(WindSource))]
public class WindVisualizer : MonoBehaviour
{
    public float density = 1f;
    public float lineLength = 2f;
    public float lineThickness = 0.1f;
    [Range(0.1f, 1f)] public float lineRoundness = 0.5f;
    public Color lineColor = Color.white;
    public float scrollSpeed = 2f;
    public float repositionRate = 0.3f;
    [Range(0f, 1f)] public float reverseLineRatio = 0.3f;

    private WindSource windSource;
    private List<LineRenderer> windLines = new List<LineRenderer>();
    private List<Vector3> localPositions = new List<Vector3>();
    private List<bool> isReversed = new List<bool>();
    private Material lineMaterial;

    private void Awake()
    {
        windSource = GetComponent<WindSource>();
        CreateLineMaterial();
        GenerateWindLines();
    }

    private void CreateLineMaterial()
    {
        lineMaterial = new Material(Shader.Find("Unlit/Transparent"));
        lineMaterial.mainTexture = CreateRoundTexture(32);
        lineMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    private Texture2D CreateRoundTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float radius = size / 2f;
        
        for(int y = 0; y < size; y++) {
            for(int x = 0; x < size; x++) {
                float dx = x - radius;
                float dy = y - radius;
                float dist = Mathf.Sqrt(dx*dx + dy*dy);
                float alpha = Mathf.Clamp01(1 - Mathf.Abs(dist - radius * lineRoundness));
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    private void GenerateWindLines()
    {
        ClearExistingLines();
        
        int lineCount = Mathf.RoundToInt(density * (windSource.shape == WindSource.WindShape.Cone ? 
            Mathf.PI * (windSource.coneDiameter/2f) * windSource.strength : 
            windSource.squareSideLength * windSource.squareSideLength));

        for(int i = 0; i < lineCount; i++) {
            Vector3 localPos = GetRandomLocalPosition();
            bool reverse = Random.value < reverseLineRatio;
            CreateWindLine(localPos, reverse);
        }
    }

    private Vector3 GetRandomLocalPosition()
    {
        if(windSource.shape == WindSource.WindShape.Cone) {
            float distance = Random.Range(0f, windSource.strength);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = (windSource.coneDiameter/2f) * (distance/windSource.strength);
            
            return new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                distance
            );
        }
        else {
            return new Vector3(
                Random.Range(-windSource.squareSideLength/2f, windSource.squareSideLength/2f),
                Random.Range(-windSource.squareSideLength/2f, windSource.squareSideLength/2f),
                Random.Range(0f, windSource.strength)
            );
        }
    }

    private void CreateWindLine(Vector3 localPosition, bool reverse)
    {
        GameObject lineObj = new GameObject("WindLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
        lr.positionCount = 2;
        
        localPositions.Add(localPosition);
        isReversed.Add(reverse);
        windLines.Add(lr);
    }

    private void Update()
    {
        UpdateLinePositions();
        AnimateLines();
        RandomRepositionLines();
    }

    private void UpdateLinePositions()
    {
        for(int i = 0; i < windLines.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(localPositions[i]);
            Vector3 direction = isReversed[i] ? -transform.forward : transform.forward;
            windLines[i].SetPosition(0, worldPos);
            windLines[i].SetPosition(1, worldPos + direction * lineLength);
        }
    }

    private void AnimateLines()
    {
        float offset = Time.time * scrollSpeed % 1f;
        lineMaterial.mainTextureOffset = new Vector2(offset, 0);
    }

    private void RandomRepositionLines()
    {
        if(Random.value < repositionRate * Time.deltaTime)
        {
            for(int i = 0; i < windLines.Count; i++)
            {
                if(Random.value < 0.3f)
                {
                    localPositions[i] = GetRandomLocalPosition();
                    isReversed[i] = Random.value < reverseLineRatio;
                }
            }
        }
    }

    private void ClearExistingLines()
    {
        foreach(LineRenderer lr in windLines)
        {
            if(lr != null) Destroy(lr.gameObject);
        }
        windLines.Clear();
        localPositions.Clear();
        isReversed.Clear();
    }

    private void OnValidate()
    {
        if(Application.isPlaying && windLines != null)
        {
            lineMaterial.mainTexture = CreateRoundTexture(32);
            GenerateWindLines();
        }
    }
}