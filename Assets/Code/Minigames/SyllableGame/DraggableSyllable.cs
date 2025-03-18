using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DraggableSyllable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 startPosition;
    public TextMeshProUGUI syllableText;
    private RawImage syllableImage;

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
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Si el objeto no está en un DropSlot válido (no es hijo del DropSlot)
        if (transform.parent == originalParent.root)
        {
            // Regresar la sílaba a su posición inicial
            transform.SetParent(originalParent);
            transform.localPosition = startPosition;
            SetWhite();
        }
    }

    // Método para cambiar el color cuando está en el lugar correcto
    public void SetGreen()
    {
        if (syllableImage != null)
        {
            Debug.Log("Estoy en el color verde");
            syllableImage.color = Color.green; // Cambiar el color a verde
        }
    }

    // Método para restablecer el color a blanco
    public void SetWhite()
    {
        if (syllableImage != null)
        {
            Debug.Log("Estoy en el color blanco");
            syllableImage.color = Color.white; // Cambiar el color a blanco
        }
    }

    public void resetAlpha()
    {
        canvasGroup.alpha = 1f;
    }
}
