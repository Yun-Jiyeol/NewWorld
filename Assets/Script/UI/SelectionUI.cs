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
    public Material UIMaterial;
}

public enum SelectionUIenum
{
    None,
    Setting,
    ChooseHowToMake,
    Color
}

public class SelectionUI : MonoBehaviour
{
    public List<UIBtnsSelectedUIMaterial> uIBtnsSelectedUIMaterials;

    public SelectionUIenum nowUIenum = SelectionUIenum.None;

    public Image BackGround;

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
                PopUpSettingPanel();
                break;
        }
    }

    void PopUpSettingPanel()
    {
        UIBtnsSelectedUIMaterial _slect = Findnowenum();
        if(_slect == null) return;

        gameObject.GetComponent<RectTransform>().DOAnchorPosX(-350f, 0.5f);
        BackGround.material = _slect.UIMaterial;

        OffAllUI();
        _slect.UISelected.SetActive(true);

        _slect.UISelected.TryGetComponent<BasicUI>(out BasicUI _basic);
        if (_basic != null)
        {
            _slect.UISelected.GetComponent<BasicUI>().OnSetting();
        }
    }

    void PopInSettingPanel()
    {
        gameObject.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.25f);
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
