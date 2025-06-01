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

        if (transform.parent == originalParent.root)
        {
            transform.SetParent(originalParent);
            transform.localPosition = startPosition;
            SetWhite();

            OnSyllablePlacedUncorrectly?.Invoke();
        }
    }

    public void SetGreen()
    {
        if (syllableImage != null)
        {
            syllableImage.color = Color.green;
        }
    }

    public void SetWhite()
    {
        if (syllableImage != null)
        {
            syllableImage.color = Color.white;
        }
    }

    public void resetAlpha()
    {
        canvasGroup.alpha = 1f;
    }
}
