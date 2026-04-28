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

    private void Awake()
    {
        thistransform = gameObject.GetComponent<RectTransform>();
        thistransform.localScale = Vector3.zero;
    }

    public override void GetParent(GameObject _parent)
    {
        parent = _parent;
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
        thistransform.DOScale(Vector2.one, 0.5f);
    }

    void EndMove()
    {
        thistransform.DOScale(Vector2.zero, 0.5f);
    }
}
