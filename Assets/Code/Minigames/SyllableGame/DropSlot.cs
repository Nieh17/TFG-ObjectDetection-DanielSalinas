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
            if (draggable.syllableText.text.Equals(expectedSyllable))
            {
                draggable.transform.SetParent(transform);
                draggable.transform.localPosition = Vector3.zero;
                draggable.resetAlpha();
                draggable.SetGreen();
                draggable.enabled = false;

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




