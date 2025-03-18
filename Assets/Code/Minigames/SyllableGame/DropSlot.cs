using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public string expectedSyllable;
    private DraggableSyllable currentSyllable;

    public static event UnityAction OnSyllablePlacedCorrectly;


    public void OnDrop(PointerEventData eventData)
    {
        DraggableSyllable draggable = eventData.pointerDrag.GetComponent<DraggableSyllable>();

        if (draggable != null)
        {
            // Si la sílaba es correcta, la colocamos en el DropSlot
            if (draggable.syllableText.text == expectedSyllable)
            {
                draggable.transform.SetParent(transform);
                draggable.transform.localPosition = Vector3.zero;  // Colocar en el centro del DropSlot
                draggable.resetAlpha();
                draggable.SetGreen(); // Cambiar color a verde al estar en el lugar correcto
                draggable.enabled = false;  // Bloquear el movimiento de la sílaba

                currentSyllable = draggable;

                OnSyllablePlacedCorrectly?.Invoke();
            }
        }
    }

    public bool IsCorrectlyFilled()
    {
        return currentSyllable != null;
    }
}
