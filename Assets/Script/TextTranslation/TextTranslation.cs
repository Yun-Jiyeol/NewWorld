using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextTranslation : MonoBehaviour
{
    [Header("NowUIText")]
    public TranslationData TD;
    public StringFont FontSave;

    TextMeshProUGUI _text;
    TranslationManager translationManager = null;

    public UnityEvent AdditionalEffect;


    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }


    private void OnEnable()
    {
        if (translationManager == null) return;

        translationManager.AddText(this);
    }


    private void Start()
    {
        translationManager = GameManager.Instance.TranslationManager;
        translationManager.AddText(this);
    }


    public void Setting(LanguageEnum _language)
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        if (AdditionalEffect != null)
        {
            AdditionalEffect?.Invoke();
        }

        ApplyTranslation(_language);
    }


    // ================================
    // Editor 미리보기 전용
    // ================================
    public void PreviewSetting(LanguageEnum _language)
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        // 제작 중에는 AdditionalEffect 실행하지 않음
        ApplyTranslation(_language);
    }


    // ================================
    // 실제 언어 적용
    // ================================
    void ApplyTranslation(LanguageEnum _language)
    {
        if (_text == null) return;
        if (TD == null) return;
        if (FontSave == null) return;


        switch (_language)
        {
            case LanguageEnum.Com:

                _text.text = TD.Strings.Com;
                _text.font = FontSave.Comfont;
                _text.fontSize = FontSave.ComSize;

                break;


            case LanguageEnum.Kor:

                _text.text = TD.Strings.Kor;
                _text.font = FontSave.Korfont;
                _text.fontSize = FontSave.KorSize;

                break;


            case LanguageEnum.Eng:

                _text.text = TD.Strings.Eng;
                _text.font = FontSave.Engfont;
                _text.fontSize = FontSave.EngSize;

                break;


            case LanguageEnum.Chs:

                _text.text = TD.Strings.Chs;
                _text.font = FontSave.Chsfont;
                _text.fontSize = FontSave.ChsSize;

                break;


            case LanguageEnum.Cht:

                _text.text = TD.Strings.Cht;
                _text.font = FontSave.Chtfont;
                _text.fontSize = FontSave.ChtSize;

                break;


            case LanguageEnum.Jpn:

                _text.text = TD.Strings.Jpn;
                _text.font = FontSave.Jpnfont;
                _text.fontSize = FontSave.JpnSize;

                break;
        }
    }


    private void OnDisable()
    {
        if (translationManager == null) return;

        translationManager.DelectText(this);
    }
}