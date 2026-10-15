using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUI : NeedToDoOnit
{
    public Transform SpawnTransform;

    public GameObject SoundSettingPage;
    public GameObject KeySettingPage;
    public GameObject LanguageSettingPage;
    public GameObject WorldSettingPage;
    public GameObject SensitivePage;
    public GameObject ScreenSettingPage;

    public GameObject WorldSettingBtn;

    [Header("Btns")]
    public UI_BtnEffect SoundBtn;
    public UI_BtnEffect KeyBtn;
    public UI_BtnEffect LanguageBtn;
    public UI_BtnEffect WorlBtn;

    int nowpage = -1;
    GameObject NowPage = null;

    public override void Onit()
    {
        ClickSettingPageBtn(0);

        if(GameManager.Instance.flow == GameFlow.Start)
        {
            WorldSettingBtn.SetActive(false);
        }
    }

    public void ClickOffBtn()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }

    /*
     * 0. sound세팅
     * 1. 키 세팅
     * 2. 언어 세팅
     * 3. 월드 세팅
     */
    public void ClickSettingPageBtn(int num) 
    {
        if(nowpage == num) return;
        nowpage = num;

        if(NowPage != null)
        {
            Destroy(NowPage);
        }

        UnHoldAll();

        switch (num)
        {
            case 0:
                NowPage = Instantiate(SoundSettingPage, SpawnTransform);
                SoundBtn.HoldChange();
                break;
            case 1:
                NowPage = Instantiate(KeySettingPage, SpawnTransform);
                KeyBtn.HoldChange();
                break;
            case 2:
                NowPage = Instantiate(LanguageSettingPage, SpawnTransform);
                LanguageBtn.HoldChange();
                break;
            case 3:
                NowPage = Instantiate(WorldSettingPage, SpawnTransform);
                WorlBtn.HoldChange();
                break;
            case 4:
                NowPage = Instantiate(SensitivePage, SpawnTransform);
                WorlBtn.HoldChange();
                break;
            case 5:
                NowPage = Instantiate(ScreenSettingPage, SpawnTransform);
                WorlBtn.HoldChange();
                break;
        }

        NowPage.GetComponent<NeedToDoOnit>()?.Onit();
        NowPage.GetComponent<NeedToDoOnit>()?.GetParent(gameObject.transform);
    }

    void UnHoldAll()
    {
        SoundBtn.UnHoldChange();
        KeyBtn.UnHoldChange();
        LanguageBtn.UnHoldChange();
        WorlBtn.UnHoldChange();
    }

    private void OnEnable()
    {
        GameManager.Instance.InputManager.isSettingUI = true;
    }

    private void OnDestroy()
    {
        GameManager.Instance.InputManager.isSettingUI = false;
    }

    private void OnDisable()
    {
        GameManager.Instance.InputManager.isSettingUI = false;
    }
}
