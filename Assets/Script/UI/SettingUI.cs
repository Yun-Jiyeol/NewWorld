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

    public GameObject WorldSettingBtn;

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

        switch (num)
        {
            case 0:
                NowPage = Instantiate(SoundSettingPage, SpawnTransform);
                break;
            case 1:
                NowPage = Instantiate(KeySettingPage, SpawnTransform);
                break;
            case 2:
                NowPage = Instantiate(LanguageSettingPage, SpawnTransform);
                break;
            case 3:
                NowPage = Instantiate(WorldSettingPage, SpawnTransform);
                break;
        }

        NowPage.GetComponent<NeedToDoOnit>().Onit();
        NowPage.GetComponent<NeedToDoOnit>().GetParent(gameObject.transform);
    }
}
