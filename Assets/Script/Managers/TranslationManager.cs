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
    LanguageEnum NowLanguage = LanguageEnum.Com;
    public List<TextTranslation> _AllText;
}
