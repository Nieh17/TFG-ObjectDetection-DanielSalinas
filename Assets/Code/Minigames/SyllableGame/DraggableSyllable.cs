using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DraggableSyllable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 startPosition;
    public TextMeshProUGUI syllableText;
    private RawImage syllableImage;

    public static event UnityAction OnSyllablePlacedUncorrectly;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
        syllableImage = GetComponent<RawImage>();
    }

    public Vector3 StartPosition()
    {
        return startPosition;
    }

    public void SetText(string text)
    {
        syllableText.text = text;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.localPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(originalParent.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        Canvas canvas = GetComponentInParent<Canvas>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Si el objeto no est� en un DropSlot v�lido (no es hijo del DropSlot)
        if (transform.parent == originalParent.root)
        {
            // Regresar la s�laba a su posici�n inicial
            transform.SetParent(originalParent);
            transform.localPosition = startPosition;
            SetWhite();

            OnSyllablePlacedUncorrectly?.Invoke();
        }
    }

    // M�todo para cambiar el color cuando est� en el lugar correcto
    public void SetGreen()
    {
        if (syllableImage != null)
        {
            syllableImage.color = Color.green; // Cambiar el color a verde
        }
    }

    // M�todo para restablecer el color a blanco
    public void SetWhite()
    {
        if (syllableImage != null)
        {
            syllableImage.color = Color.white; // Cambiar el color a blanco
        }
    }

    public void resetAlpha()
    {
        canvasGroup.alpha = 1f;
    }
}
