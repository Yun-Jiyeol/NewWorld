using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldIconChild : NeedToDoOnit, IPointerDownHandler
{
    public Image icon;

    int SavedNum = 0;
    MakeNewWorld parent;

    public override void GetParent(GameObject _parent)
    {
        parent = _parent.GetComponent<MakeNewWorld>();
    }

    public override void Onit(int _num)
    {
        SavedNum = _num;
        icon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[SavedNum];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        parent.Onit(SavedNum);
    }
}
