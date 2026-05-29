using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    Slot originalSlot;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<Slot>();

        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        Slot targetSlot = null;

        if (eventData.pointerEnter != null)
        {
            targetSlot = eventData.pointerEnter.GetComponent<Slot>();

            if (targetSlot == null && eventData.pointerEnter.transform.parent != null)
            {
                targetSlot = eventData.pointerEnter.transform.parent.GetComponent<Slot>();
            }
        }

        if (targetSlot == null || targetSlot == originalSlot)
        {
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        }

        GameObject targetItem = targetSlot.currentItem;

        if (targetItem != null)
        {
            targetItem.transform.SetParent(originalParent);
            targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            targetSlot.currentItem = originalSlot.currentItem;
        }
        else
        {
            targetSlot.currentItem = originalSlot.currentItem;
        }
        transform.SetParent(targetSlot.transform);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        originalSlot.currentItem = targetItem;
    }
}