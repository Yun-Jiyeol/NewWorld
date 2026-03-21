using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoDrag : ScrollRect
{ 
    public override void OnBeginDrag(PointerEventData eventData)
    {
        // 아무것도 안함 → 드래그 시작 안됨
    }

    // 드래그 중 막기
    public override void OnDrag(PointerEventData eventData)
    {
        // 아무것도 안함
    }

    // 드래그 끝 막기
    public override void OnEndDrag(PointerEventData eventData)
    {
        // 아무것도 안함
    }

    public override void OnScroll(PointerEventData data)
    {
        base.OnScroll(data);
    }
}
