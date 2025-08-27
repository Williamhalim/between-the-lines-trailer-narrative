using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false; // allow drop detection
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        Transform parent = originalParent;
        int nearestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform sibling = parent.GetChild(i);
            if (sibling == transform) continue;

            float distance = Mathf.Abs(rectTransform.localPosition.y - sibling.localPosition.y);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        // Optional: adjust index if dragging below
        if (rectTransform.localPosition.y < parent.GetChild(nearestIndex).localPosition.y)
            nearestIndex += 1;

        // Set sibling index
        rectTransform.SetSiblingIndex(nearestIndex);

        // Reset local position so LayoutGroup snaps it correctly
        rectTransform.localPosition = Vector3.zero;

    }
}
