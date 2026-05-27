using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldIconParent : NeedToDoOnit
{
    public Transform SpawnIconTransform;
    public GameObject Icons;

    List<GameObject> IconsSpawned = new List<GameObject>();
    RectTransform thistransform;

    bool isOn = false;
    GameObject parent;
    MakeNewWorld parentMNW;

    private void Awake()
    {
        thistransform = gameObject.GetComponent<RectTransform>();
        thistransform.localScale = Vector3.one;
    }

    public override void GetParent(GameObject _parent)
    {
        parent = _parent;
        parentMNW = parent.GetComponent<MakeNewWorld>();
    }

    public override void Onit()
    {
        thistransform.DOKill();

        if(IconsSpawned.Count == 0)
        {
            for(int i =0; i < GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting.Count; i++)
            {
                GameObject _icon = Instantiate(Icons, SpawnIconTransform);
                _icon.GetComponent<NeedToDoOnit>().Onit(i);
                _icon.GetComponent<NeedToDoOnit>().GetParent(parent);

                IconsSpawned.Add(_icon);
            }
        }

        if (isOn)
        {
            EndMove();
        }
        else
        {
            StartMove();
        }

        isOn = !isOn;
    }

    void StartMove()
    {
        parentMNW.MainUI.DOKill();
        parentMNW.AdditionalUIIconBtnEffect.HoldChange();
        parentMNW.MainUI.DOSizeDelta(new Vector2(1175, 650), 0.5f);
    }

    void EndMove()
    {
        parentMNW.MainUI.DOKill();
        parentMNW.AdditionalUIIconBtnEffect.UnHoldChange();
        parentMNW.MainUI.DOSizeDelta(new Vector2(750, 650), 0.5f);
    }
}
