using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Desenha contornos pulsantes em células do tabuleiro e em elementos de UI.
// O contorno é gerado por código, então não precisa de arte.
public class TutorialHighlighter : MonoBehaviour
{
    [Header("Aparência")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseScale = 0.08f;

    [Header("Tabuleiro")]
    [SerializeField] private int cellSortingOrder = 1000;

    [Header("Interface")]
    [Tooltip("Distância (em pixels) entre o contorno e o elemento destacado")]
    [SerializeField] private float uiPadding = 8f;

    private const int TextureSize = 48;
    private const int BorderThickness = 6;
    private const int SliceBorder = 12;

    private Sprite worldSprite;
    private Sprite uiSprite;

    private readonly List<SpriteRenderer> cellMarkers =
        new List<SpriteRenderer>();

    private readonly List<Image> uiMarkers =
        new List<Image>();

    private Transform cellMarkersRoot;
    private bool visible = true;

    private void Awake()
    {
        CreateSprites();

        cellMarkersRoot = new GameObject("Tutorial Cell Highlights").transform;
        cellMarkersRoot.SetParent(transform, false);
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void Update()
    {
        // Usa tempo não escalonado para funcionar mesmo com o jogo pausado
        float wave =
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;

        Color color = highlightColor;
        color.a = Mathf.Lerp(0.45f, 1f, wave) * highlightColor.a;

        float scale = 1f + wave * pulseScale;

        foreach (SpriteRenderer marker in cellMarkers)
        {
            marker.color = color;
            marker.transform.localScale = Vector3.one * scale;
        }

        foreach (Image marker in uiMarkers)
        {
            if (marker == null)
                continue;

            marker.color = color;
            marker.rectTransform.localScale = Vector3.one * scale;
        }
    }


    public void HighlightCell(Vector3 worldPosition, float cellSize)
    {
        GameObject markerObject = new GameObject("Cell Highlight");
        markerObject.transform.SetParent(cellMarkersRoot, false);
        markerObject.transform.position = worldPosition;

        SpriteRenderer marker =
            markerObject.AddComponent<SpriteRenderer>();

        marker.sprite = worldSprite;
        marker.drawMode = SpriteDrawMode.Sliced;
        marker.size = new Vector2(cellSize, cellSize);
        marker.sortingOrder = cellSortingOrder;
        marker.color = highlightColor;
        marker.enabled = visible;

        cellMarkers.Add(marker);
    }


    public void HighlightUI(RectTransform target)
    {
        if (target == null)
            return;

        GameObject markerObject = new GameObject(
            "Tutorial Highlight",
            typeof(RectTransform)
        );

        RectTransform rect = markerObject.GetComponent<RectTransform>();
        rect.SetParent(target, false);

        // Estica o contorno para envolver todo o elemento
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(-uiPadding, -uiPadding);
        rect.offsetMax = new Vector2(uiPadding, uiPadding);
        rect.SetAsLastSibling();

        // Não interfere em Layout Groups
        LayoutElement layout = markerObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        Image marker = markerObject.AddComponent<Image>();
        marker.sprite = uiSprite;
        marker.type = Image.Type.Sliced;
        marker.raycastTarget = false;
        marker.color = highlightColor;
        marker.enabled = visible;

        uiMarkers.Add(marker);
    }


    public void Clear()
    {
        foreach (SpriteRenderer marker in cellMarkers)
        {
            if (marker != null)
                Destroy(marker.gameObject);
        }

        foreach (Image marker in uiMarkers)
        {
            if (marker != null)
                Destroy(marker.gameObject);
        }

        cellMarkers.Clear();
        uiMarkers.Clear();
    }


    public void SetVisible(bool isVisible)
    {
        visible = isVisible;

        foreach (SpriteRenderer marker in cellMarkers)
        {
            if (marker != null)
                marker.enabled = isVisible;
        }

        foreach (Image marker in uiMarkers)
        {
            if (marker != null)
                marker.enabled = isVisible;
        }
    }


    // Gera uma textura quadrada com apenas a borda preenchida
    private void CreateSprites()
    {
        Texture2D texture = new Texture2D(
            TextureSize,
            TextureSize,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "Tutorial Highlight Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                bool isBorder =
                    x < BorderThickness ||
                    y < BorderThickness ||
                    x >= TextureSize - BorderThickness ||
                    y >= TextureSize - BorderThickness;

                pixels[y * TextureSize + x] =
                    isBorder ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Rect rect = new Rect(0, 0, TextureSize, TextureSize);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Vector4 border = Vector4.one * SliceBorder;

        // No mundo: 1 sprite = 1 unidade (tamanho de uma célula)
        worldSprite = Sprite.Create(
            texture, rect, pivot, TextureSize,
            0, SpriteMeshType.FullRect, border
        );

        // Na UI: 100 pixels por unidade (padrão do Canvas)
        uiSprite = Sprite.Create(
            texture, rect, pivot, 100f,
            0, SpriteMeshType.FullRect, border
        );
    }
}
