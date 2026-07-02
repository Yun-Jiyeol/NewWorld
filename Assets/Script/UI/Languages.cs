using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Languages : MonoBehaviour
{
    TranslationManager translationManager;

    public UI_BtnEffect Com;
    public UI_BtnEffect Eng;
    public UI_BtnEffect Kor;

    LanguageEnum nowLanguage;

    public void Onit()
    {
        translationManager = GameManager.Instance.TranslationManager;
        nowLanguage = translationManager.GetLanguage();
        Setting();
    }

    public void ClickCOMLanguage()
    {
        ClickLanguage(LanguageEnum.Com);
    }

    public void ClickENGLanguage()
    {
        ClickLanguage(LanguageEnum.Eng);
    }

    public void ClickKORLanguage()
    {
        ClickLanguage(LanguageEnum.Kor);
    }

    void ClickLanguage(LanguageEnum _Lan)
    {
        if (_Lan == translationManager.GetLanguage()) return;

        translationManager.ChangeLanguage(_Lan);
        nowLanguage = translationManager.GetLanguage();

        GameManager.Instance.SoundManager.PlayEffect(SoundChoose.CallAudio(ClickSound.SimplyUITap));
        Setting();
    }

    void Setting()
    {
        ResetEffect();

        switch (nowLanguage)
        {
            case LanguageEnum.Com:
                Com.HoldEnter();
                break;
            case LanguageEnum.Eng:
                Eng.HoldEnter();
                break;
            case LanguageEnum.Kor:
                Kor.HoldEnter();
                break;
        }
    }

    void ResetEffect()
    {
        Com.UnHoldChange();
        Eng.UnHoldChange();
        Kor.UnHoldChange();
    }
}
