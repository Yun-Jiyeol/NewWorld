using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public enum LanguageEnum
{
    Com,
    Kor,
    Eng
}

[Serializable]
public class SaveString
{
    public string Com;
    public string Kor;
    public string Eng;
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
}

public class TranslationManager : MonoBehaviour
{
    LanguageEnum NowLanguage = LanguageEnum.Kor;
    public List<TextTranslation> _AllText;

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

        if (_AllText.Count == 0) return;
        foreach(TextTranslation _tt in _AllText)
        {
            _tt.Setting(NowLanguage);
        }
    }

    public LanguageEnum GetLanguage()
    {
        return NowLanguage;
    }
}
