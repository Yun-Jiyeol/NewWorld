using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIBtnsSelectedUIMaterial
{
    public SelectionUIenum UIenum;
    public GameObject Btn;
    public GameObject UISelected;
}

public enum SelectionUIenum
{
    None,
    Setting,
    ChooseHowToMake,
    Color,
    LightOneWorld,
    MoveOneWorld,
    MainLight,
    PlayerPos,
    WorldSetting,
    PutWorld,
    CameraSetting,
    HelpingInMake
}

public class SelectionUI : MonoBehaviour
{
    public List<UIBtnsSelectedUIMaterial> uIBtnsSelectedUIMaterials;

    public SelectionUIenum nowUIenum = SelectionUIenum.None;

    public Image BackGround;

    RectTransform thistransform;
    public RectTransform Seletions;
    public RectTransform SelectPage;
    float size = 350f;

    public List<GameObject> AdditionalUI = new List<GameObject>();

    private void Start()
    {
        thistransform = gameObject.GetComponent<RectTransform>();
        SettingAll();
    }

    void SettingAll()
    {
        for(int i = 0; i< uIBtnsSelectedUIMaterials.Count; i++)
        {
            uIBtnsSelectedUIMaterials[i].UISelected.TryGetComponent<BasicUI>(out BasicUI _basic);
            if (_basic != null)
            {
                uIBtnsSelectedUIMaterials[i].UISelected.SetActive(true);
                _basic.OnSetting();
            }
            uIBtnsSelectedUIMaterials[i].UISelected.SetActive(false);
        }
    }

    public void ChangeNowUIEnum(SelectionUIenum _changeenum)
    {
        nowUIenum = _changeenum;

        switch (nowUIenum)
        {
            case SelectionUIenum.None:
                PopInSettingPanel();
                break;

            default:
                PopUpSettingPanel();
                break;
        }
    }

    public void PopUpSettingPanel()
    {
        UIBtnsSelectedUIMaterial _slect = Findnowenum();

        thistransform.DOKill();
        thistransform.DOAnchorPosX(-size, 0.5f);
        DestroyAllAdditionalUi();

        OffAllUI();
        if(_slect == null) return;

        _slect.UISelected.SetActive(true);

        _slect.UISelected.TryGetComponent<BasicUI>(out BasicUI _basic);
        if (_basic != null)
        {
            _basic.OnSetting();
        }
    }

    public void PopInSettingPanel()
    {
        thistransform.DOKill();
        thistransform.DOAnchorPosX(0f, 0.25f);
        DestroyAllAdditionalUi();

        for (int i = 0; i < uIBtnsSelectedUIMaterials.Count; i++)
        {
            uIBtnsSelectedUIMaterials[i].UISelected.TryGetComponent<BasicUI>(out BasicUI _basic);
            if (_basic != null)
            {
                uIBtnsSelectedUIMaterials[i].UISelected.SetActive(false);
            }
        }
    }

    UIBtnsSelectedUIMaterial Findnowenum()
    {
        foreach(UIBtnsSelectedUIMaterial _select in uIBtnsSelectedUIMaterials)
        {
            if(_select.UIenum == nowUIenum)
            {
                return _select;
            }
        }

        return null;
    }

    void OffAllUI()
    {
        foreach (UIBtnsSelectedUIMaterial _select in uIBtnsSelectedUIMaterials)
        {
            _select.UISelected.SetActive(false);
        }
    }

    public void ChooseToOnUIBtn(List<SelectionUIenum> _list)
    {
        PopInSettingPanel();

        if (_list == null)
        {
            foreach (UIBtnsSelectedUIMaterial _select in uIBtnsSelectedUIMaterials)
            {
                _select.Btn.SetActive(false);
            }
            return;
        }

        foreach (UIBtnsSelectedUIMaterial _select in uIBtnsSelectedUIMaterials)
        {
            if (_list.Contains(_select.UIenum))
            {
                _select.Btn.SetActive(true);
            }
            else
            {
                _select.Btn.SetActive(false);
            }
        }
    }

    public void DestroyAllAdditionalUi()
    {
        if(AdditionalUI.Count == 0) return;
        foreach(GameObject go in AdditionalUI)
        {
            Destroy(go);
        }
        AdditionalUI.Clear();
    }

    public float scaleratio = 1f;
    float Originalsize = 350f;
    public Action OnChangeUIScale;

    public void OnChangeUISize(Vector2 _size)
    {
        size = _size.x;

        SelectPage.sizeDelta = _size;

        thistransform.anchoredPosition = new Vector2(-size, 0);

        SelectPage.anchoredPosition = new Vector2(size, 0);

        scaleratio = size/Originalsize;

        OnChangeUIScale?.Invoke();
    }
}
