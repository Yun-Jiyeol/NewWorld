using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldIconChild : NeedToDoOnit, IPointerDownHandler
{
    public Image icon;

    int SavedNum = 0;
    GameObject parent;

    public override void GetParent(GameObject _parent)
    {
        parent = _parent;
    }

    public override void Onit(int _num)
    {
        SavedNum = _num;
        icon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[SavedNum];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        parent.GetComponent<NeedToDoOnit>().Onit(SavedNum);
    }
}
