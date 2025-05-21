using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragTriggerAnimator : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Animator animator;
    public float dragThreshold = 30f;

    private Vector2 startPos;
    private bool dragTriggered = false;

    public Image dragImage;

    public void OnPointerDown(PointerEventData eventData)
    {
        dragImage.gameObject.SetActive(false);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out startPos
        );
        dragTriggered = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragTriggered) return;

        Vector2 currentPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out currentPos
        );

        float deltaY = currentPos.y - startPos.y;

        if (Mathf.Abs(deltaY) > dragThreshold)
        {
            if (deltaY < 0f)
            {
                animator.SetTrigger("Drag_Down");
            }
            else
            {
                animator.SetTrigger("Drag_Up");
            }

            dragTriggered = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragTriggered = false;
    }
}
