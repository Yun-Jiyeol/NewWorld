using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public enum LanguageEnum
{
    Com,
    Kor,
    Eng,
    Chs,
    Cht
}

[Serializable]
public class SaveString
{
    public string Com;
    public string Kor;
    public string Eng; 
    public string Chs;
    public string Cht;
}

[System.Serializable]
public class StringFont
{
    public TMP_FontAsset Comfont;
    public float ComSize;
    
    public TMP_FontAsset Korfont;
    public float KorSize;

    public TMP_FontAsset Engfont;
    public float EngSize;

    public TMP_FontAsset Chsfont;
    public float ChsSize;

    public TMP_FontAsset Chtfont;
    public float ChtSize;
}

public class TranslationManager : MonoBehaviour
{
    LanguageEnum NowLanguage = LanguageEnum.Com;
    public List<TextTranslation> _AllText;
    public event Action<LanguageEnum> OnLanguageChanged;

    public void AddText(TextTranslation _text)
    {
        if (_AllText.Contains(_text)) return;

        _AllText.Add(_text);
        _text.Setting(NowLanguage);
    }

    public void DelectText(TextTranslation _text)
    {
        if (!_AllText.Contains(_text)) return;

        _AllText.Remove(_text);
    }

    public void ChangeLanguage(LanguageEnum _LE)
    {
        NowLanguage = _LE;

        // 기존 TextTranslation 갱신
        if (_AllText != null)
        {
            foreach (TextTranslation _tt in _AllText)
            {
                if (_tt != null)
                {
                    _tt.Setting(NowLanguage);
                }
            }
        }

        // TextTranslation이 아닌 다른 UI들에게도 알림
        OnLanguageChanged?.Invoke(NowLanguage);
    }

    public LanguageEnum GetLanguage()
    {
        return NowLanguage;
    }
}
