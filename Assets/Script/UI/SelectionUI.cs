using DG.Tweening;
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
    MainLight
}

public class SelectionUI : MonoBehaviour
{
    public List<UIBtnsSelectedUIMaterial> uIBtnsSelectedUIMaterials;

    public SelectionUIenum nowUIenum = SelectionUIenum.None;

    public Image BackGround;

    RectTransform thistransform;

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

            case SelectionUIenum.ChooseHowToMake:
            case SelectionUIenum.Color:
            case SelectionUIenum.Setting:
            case SelectionUIenum.LightOneWorld:
            case SelectionUIenum.MoveOneWorld:
            case SelectionUIenum.MainLight:
                PopUpSettingPanel();
                break;
        }
    }

    public void PopUpSettingPanel()
    {
        UIBtnsSelectedUIMaterial _slect = Findnowenum();

        thistransform.DOKill();
        thistransform.DOAnchorPosX(-350f, 0.5f);

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
}
