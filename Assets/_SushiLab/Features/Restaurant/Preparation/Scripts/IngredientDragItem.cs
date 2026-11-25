
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngredientDragItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Datos del ingrediente")]
    [SerializeField] private IngredientData ingredientData;
    [SerializeField] private Image ingredientImage;
    [Header("Imagen que se arrastra")]
    [SerializeField] private Sprite dragSprite;

    private Canvas rootCanvas;
    private RectTransform dragPreview;

    public IngredientData Ingredient => ingredientData;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ingredientData == null)
        {
            Debug.LogWarning("Este ingrediente no tiene IngredientData.");
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        rootCanvas = canvas.rootCanvas;

        // Creamos una imagen temporal para seguir al cursor.
        GameObject preview = new GameObject(
            "DragPreview",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        preview.transform.SetParent(rootCanvas.transform, false);
        preview.transform.SetAsLastSibling();

        dragPreview = preview.GetComponent<RectTransform>();
        dragPreview.sizeDelta = new Vector2(180, 130);

        Image image = preview.GetComponent<Image>();

        image.raycastTarget = false;
        image.preserveAspect = true;

        // Utilizamos el sprite de la porcion que se arrastra.
        // Si no esta asignado, usamos la imagen del mostrador.
        Sprite previewSprite = dragSprite != null
            ? dragSprite
            : (ingredientImage != null ? ingredientImage.sprite : null);

        if (previewSprite != null)
        {
            image.sprite = previewSprite;
            image.color = Color.white;
        }
        else
        {
            // Apariencia temporal mientras no tengamos sprites.
            image.color = new Color(0.95f, 0.84f, 0.7f, 0.9f);

            GameObject labelObject = new GameObject(
                "Text_Preview",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            labelObject.transform.SetParent(preview.transform, false);

            RectTransform labelRect =
                labelObject.GetComponent<RectTransform>();

            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label =
                labelObject.GetComponent<TextMeshProUGUI>();

            label.text = ingredientData.displayName;
            label.fontSize = 20;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.35f, 0.24f, 0.16f);
            label.raycastTarget = false;
        }

        MovePreview(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MovePreview(eventData);
    }

    private void MovePreview(PointerEventData eventData)
    {
        if (dragPreview == null || rootCanvas == null)
            return;

        RectTransform canvasRect =
            rootCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPosition))
        {
            dragPreview.anchoredPosition = localPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreview != null)
            Destroy(dragPreview.gameObject);

        dragPreview = null;
    }

    private void OnDisable()
    {
        if (dragPreview != null)
            Destroy(dragPreview.gameObject);

        dragPreview = null;
    }
}
