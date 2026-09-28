using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldExUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    ChunkCode ChunkCode;

    public TextMeshProUGUI Name;
    public TextMeshProUGUI xpos;
    public TextMeshProUGUI ypos;
    public TextMeshProUGUI zpos;


    [Header("Translation")]
    public TranslationData XPositionText;
    public TranslationData YPositionText;
    public TranslationData ZPositionText;


    public GameObject OnSelect;
    public GameObject OnMouse;
    public GameObject OnError;


    PlayerPosUI PPUI;

    TranslationManager translationManager;

    Transform worldTransform;


    public void Setting(GameObject _world, PlayerPosUI _ppui)
    {
        PPUI = _ppui;
        ChunkCode = _world.GetComponent<ChunkCode>();

        RefreshLoadState();

        Name.text = ChunkCode._name;

        xpos.text =
            "X좌표 = " +
            _world.transform.position.x.ToString("F1");

        ypos.text =
            "Y좌표 = " +
            _world.transform.position.y.ToString("F1");

        zpos.text =
            "Z좌표 = " +
            _world.transform.position.z.ToString("F1");
    }

    void Update()
    {
        RefreshLoadState();
    }

    void RefreshLoadState()
    {
        if (ChunkCode == null)
        {
            OnError.SetActive(true);
            return;
        }

        OnError.SetActive(
            !ChunkCode.IsLoadComplete()
        );
    }

    public bool CanSelect()
    {
        if (ChunkCode == null)
            return false;

        return ChunkCode.IsLoadComplete();
    }


    // =========================
    // 언어 변경 이벤트 연결
    // =========================

    void SubscribeLanguage()
    {
        if (translationManager == null)
            return;

        // 중복 등록 방지
        translationManager.OnLanguageChanged -= OnLanguageChanged;
        translationManager.OnLanguageChanged += OnLanguageChanged;
    }


    void OnLanguageChanged(LanguageEnum language)
    {
        RefreshPositionText(language);
    }


    // =========================
    // 위치 텍스트 다시 출력
    // =========================

    void RefreshPositionText(LanguageEnum language)
    {
        if (worldTransform == null)
            return;


        xpos.text =
            GetTranslation(XPositionText, language)
            + " = "
            + worldTransform.position.x.ToString("F1");


        ypos.text =
            GetTranslation(YPositionText, language)
            + " = "
            + worldTransform.position.y.ToString("F1");


        zpos.text =
            GetTranslation(ZPositionText, language)
            + " = "
            + worldTransform.position.z.ToString("F1");
    }


    // =========================
    // ScriptableObject 번역
    // =========================

    string GetTranslation(
        TranslationData data,
        LanguageEnum language
    )
    {
        if (data == null)
            return "";


        SaveString text = data.Strings;


        switch (language)
        {
            case LanguageEnum.Com:
                return text.Com;

            case LanguageEnum.Kor:
                return text.Kor;

            case LanguageEnum.Eng:
                return text.Eng;

            case LanguageEnum.Chs:
                return text.Chs;

            case LanguageEnum.Cht:
                return text.Cht;

            case LanguageEnum.Jpn:
                return text.Jpn;
        }


        return "";
    }


    // =========================
    // 이벤트 해제
    // =========================

    private void OnDestroy()
    {
        if (translationManager != null)
        {
            translationManager.OnLanguageChanged
                -= OnLanguageChanged;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouse.SetActive(true);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouse.SetActive(false);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        PPUI.TryToLookAt(this);
    }


    public void LooAtThis()
    {
        if (!CanSelect())
            return;

        OnSelect.SetActive(true);

        GameManager.Instance.MainCamera
            .LookAt(ChunkCode.gameObject.transform);
    }


    public ChunkCode getChunkCode()
    {
        return ChunkCode;
    }
}