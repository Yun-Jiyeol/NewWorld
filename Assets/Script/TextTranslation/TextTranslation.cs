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
        if (AdditionalEffect != null) AdditionalEffect?.Invoke();


        if (TD == null) return;

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
        translationManager.DelectText(this);
    }
}
