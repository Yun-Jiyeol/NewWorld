using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    HelpingInMake,
    SetWorldPosition,
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

    [Header("Explain")]
    public TextMeshProUGUI ExplainText;
    public Image ExplainImage;

    bool isPopUp = false;
    public SoundData PopUpUI;
    public SoundData PopInUI;

    public void Onit()
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
        if(nowUIenum == _changeenum)
        {
            PopInSettingPanel();

            return;
        }

        nowUIenum = _changeenum;

        switch (nowUIenum)
        {
            case SelectionUIenum.None:
                PopInSettingPanel();
                break;

            case SelectionUIenum.Setting:
                GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.SettingUI);
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

        if (!isPopUp)
        {
            thistransform.DOKill();
            thistransform.DOAnchorPosX(-145f - size, 0.5f);
            GameManager.Instance.SoundManager.PlayEffect(PopUpUI);
        }
        isPopUp = true;

        DestroyAllAdditionalUi();

        OffAllUI();
        if(_slect == null) return;

        _slect.UISelected.SetActive(true);

        _slect.UISelected.TryGetComponent<BasicUI>(out BasicUI _basic);
        if (_basic != null)
        {
            _basic.OnSetting();
        }

        Settingtext(_slect);
    }

    public void Settingtext(UIBtnsSelectedUIMaterial _slect)
    {
        UI_BtnEffect _UIBE = _slect.Btn.GetComponent<UI_BtnEffect>();
        ExplainText.text = _UIBE._text.text;
        ExplainImage.sprite = _UIBE._image2.sprite;
    }

    public void Settingtext()
    {
        UIBtnsSelectedUIMaterial _slect = Findnowenum();
        if (_slect == null) return; 

        UI_BtnEffect _UIBE = _slect.Btn.GetComponent<UI_BtnEffect>();
        ExplainText.text = _UIBE._text.text;
        ExplainImage.sprite = _UIBE._image2.sprite;
    }

    public void PopInSettingPanel()
    {
        nowUIenum = SelectionUIenum.None;

        if (isPopUp)
        {
            thistransform.DOKill();
            thistransform.DOAnchorPosX(-145f, 0.25f);
            GameManager.Instance.SoundManager.PlayEffect(PopInUI);
        }
        isPopUp = false;

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
        return;

        size = _size.x;

        SelectPage.sizeDelta = _size;

        thistransform.anchoredPosition = new Vector2(-size, 0);

        SelectPage.anchoredPosition = new Vector2(size, 0);

        scaleratio = size/Originalsize;

        OnChangeUIScale?.Invoke();
    }
}
