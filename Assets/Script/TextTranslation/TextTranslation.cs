using TMPro;
using UnityEngine;

public class TextTranslation : MonoBehaviour
{
    [Header("NowUIText")]
    public TranslationData TD;
    public StringFont FontSave;

    TextMeshProUGUI _text;
    TranslationManager translationManager;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if(!translationManager._AllText.Contains(this))
            translationManager._AllText.Add(this);
    }

    private void Start()
    {
        translationManager = GameManager.Instance.TranslationManager;
    }

    public void Setting(LanguageEnum _language)
    {
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
        }
    }

    private void OnDisable()
    {
        if (translationManager._AllText.Contains(this))
            translationManager._AllText.Remove(this);
    }
}
