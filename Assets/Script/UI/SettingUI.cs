using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUI : NeedToDoOnit
{
    public Transform SpawnTransform;

    public GameObject SoundSettingPage;

    int nowpage = -1;
    GameObject NowPage = null;

    public override void Onit()
    {
        ClickSettingPageBtn(0);
    }

    public void ClickOffBtn()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }

    /*
     * 0. sound세팅
     * 1. 키 세팅
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
        }

        NowPage.GetComponent<NeedToDoOnit>().Onit();
    }
}
