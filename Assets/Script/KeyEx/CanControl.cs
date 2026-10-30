using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanControl : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("필요할 때 생성하고 보관할 KeyEx Prefab")]
    public KeyEx KeyExPrefab;

    [Header("Shared Sprites")]
    [Tooltip("Range 표시용 공통 ~ Sprite")]
    public Sprite RangeSprite;

    [Tooltip("Mouse0 대체 Sprite")]
    public Sprite Mouse0Sprite;

    [Tooltip("Mouse1 대체 Sprite")]
    public Sprite Mouse1Sprite;

    [Tooltip("Mouse2 대체 Sprite")]
    public Sprite Mouse2Sprite;

    [Tooltip("Mouse3 대체 Sprite")]
    public Sprite Mouse3Sprite;

    [Tooltip("Mouse4 대체 Sprite")]
    public Sprite Mouse4Sprite;

    [Tooltip("Mouse5 대체 Sprite")]
    public Sprite Mouse5Sprite;

    [Tooltip("Mouse6 대체 Sprite")]
    public Sprite Mouse6Sprite;

    [Tooltip("마우스 휠 스크롤 입력 대체 Sprite. 비어 있으면 텍스트로 표시")]
    public Sprite MouseWheelSprite;

    [Header("Range Sprite Size")]
    [Tooltip("Range 표시용 ~ Sprite의 가로 크기")]
    public float RangeSpriteWidth = 20f;

    [Tooltip("Range 표시용 ~ Sprite의 세로 크기")]
    public float RangeSpriteHeight = 10f;

    [Header("Mouse Sprite Size")]
    [Tooltip("마우스 클릭 Sprite의 가로 크기")]
    public float MouseSpriteWidth = 20f;

    [Tooltip("마우스 클릭 Sprite의 세로 크기. 0 이하이면 현재 템플릿 높이 유지")]
    public float MouseSpriteHeight = 0f;

    [Header("Shift Input Color")]
    [Tooltip("shiftClick=true인 실제 키(예: 저장용 1~6)의 배경 Image에 적용할 색")]
    public Color ShiftKeyImageColor =
        new Color(0.45f, 0.45f, 0.45f, 1f);

    [Tooltip("shiftClick=true인 실제 키(예: 저장용 1~6)의 TextMeshProUGUI에 적용할 색")]
    public Color ShiftKeyTextColor =
        new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("State ScriptableObjects")]
    [Tooltip("위에서부터 검사하고, 조건이 맞는 모든 State의 Rows를 합쳐서 표시")]
    public List<CanControlStateData> States =
        new List<CanControlStateData>();

    [Header("Size")]
    public float OneKeyHeight = 30f;
    public float ChangeHeightTime = 0.2f;

    private RectTransform thisRect;

    private readonly List<KeyEx> keyPool =
        new List<KeyEx>();

    private GameFlow beforeFlow =
        (GameFlow)(-1);

    private SelectionUIenum beforeUI =
        (SelectionUIenum)(-1);

    private CoinType beforeMoveType =
        (CoinType)(-1);

    private LanguageEnum beforeLanguage =
        (LanguageEnum)(-1);

    private void Awake()
    {
        thisRect =
            GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        GameFlow nowFlow =
            GameManager.Instance.flow;

        SelectionUIenum nowUI =
            GetNowUI();

        CoinType nowMoveType =
            GetNowMoveType();

        LanguageEnum nowLanguage =
            beforeLanguage;

        if (GameManager.Instance
            .TranslationManager != null)
        {
            nowLanguage =
                GameManager.Instance
                    .TranslationManager
                    .GetLanguage();
        }

        if (beforeFlow != nowFlow ||
            beforeUI != nowUI ||
            beforeMoveType != nowMoveType ||
            beforeLanguage != nowLanguage)
        {
            Refresh();
        }
    }

    public void RefreshKeySetting()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (GameManager.Instance == null)
            return;

        GameFlow nowFlow =
            GameManager.Instance.flow;

        SelectionUIenum nowUI =
            GetNowUI();

        CoinType nowMoveType =
            GetNowMoveType();

        beforeFlow = nowFlow;
        beforeUI = nowUI;
        beforeMoveType = nowMoveType;

        if (GameManager.Instance
            .TranslationManager != null)
        {
            beforeLanguage =
                GameManager.Instance
                    .TranslationManager
                    .GetLanguage();
        }

        int activeCount =
            ShowMatchingStates(
                nowFlow,
                nowUI,
                nowMoveType
            );

        HideUnusedKeyEx(
            activeCount
        );

        ChangeHeight(
            activeCount
        );
    }

    private int ShowMatchingStates(
        GameFlow flow,
        SelectionUIenum ui,
        CoinType moveType)
    {
        if (States == null)
            return 0;

        int activeCount = 0;

        // States의 위에서부터 순서대로 검사.
        // 조건이 맞는 State를 하나만 고르는 것이 아니라
        // 맞는 State의 Rows를 전부 이어서 표시한다.
        for (int stateIndex = 0;
             stateIndex < States.Count;
             stateIndex++)
        {
            CanControlStateData state =
                States[stateIndex];

            if (state == null)
                continue;

            if (!state.IsMatch(
                flow,
                ui,
                moveType))
            {
                continue;
            }

            if (state.Rows == null)
                continue;

            for (int rowIndex = 0;
                 rowIndex < state.Rows.Count;
                 rowIndex++)
            {
                CanControlRowData row =
                    state.Rows[rowIndex];

                if (row == null)
                    continue;

                KeyEx keyEx =
                    GetKeyEx(
                        activeCount
                    );

                if (keyEx == null)
                    continue;

                keyEx.gameObject
                    .SetActive(true);

                keyEx.transform
                    .SetSiblingIndex(
                        activeCount
                    );

                keyEx.Setting(
                    row,
                    GetTranslatedText(
                        row.ExplainText
                    ),
                    this
                );

                activeCount++;
            }
        }

        return activeCount;
    }

    private KeyEx GetKeyEx(
        int index)
    {
        while (keyPool.Count <= index)
        {
            if (KeyExPrefab == null)
            {
                Debug.LogWarning(
                    "[CanControl] KeyExPrefab이 연결되지 않았습니다."
                );

                return null;
            }

            KeyEx newKeyEx =
                Instantiate(
                    KeyExPrefab,
                    transform
                );

            newKeyEx.name =
                "KeyEx_" +
                keyPool.Count;

            newKeyEx.gameObject
                .SetActive(false);

            keyPool.Add(
                newKeyEx
            );
        }

        return keyPool[index];
    }

    private void HideUnusedKeyEx(
        int startIndex)
    {
        for (int i = startIndex;
             i < keyPool.Count;
             i++)
        {
            KeyEx keyEx =
                keyPool[i];

            if (keyEx == null)
                continue;

            keyEx.ResetKey();

            keyEx.gameObject
                .SetActive(false);
        }
    }

    public Sprite GetInputSprite(
        Keyboards keyboard)
    {
        if (keyboard == null)
            return null;

        switch (keyboard.inputType)
        {
            case KeyInputType.MouseWheel:
                return MouseWheelSprite;
        }

        return GetMouseSprite(
            keyboard.key
        );
    }

    public Sprite GetMouseSprite(
        KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Mouse0:
                return Mouse0Sprite;

            case KeyCode.Mouse1:
                return Mouse1Sprite;

            case KeyCode.Mouse2:
                return Mouse2Sprite;

            case KeyCode.Mouse3:
                return Mouse3Sprite;

            case KeyCode.Mouse4:
                return Mouse4Sprite;

            case KeyCode.Mouse5:
                return Mouse5Sprite;

            case KeyCode.Mouse6:
                return Mouse6Sprite;
        }

        return null;
    }

    private SelectionUIenum GetNowUI()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.UIManager == null ||
            GameManager.Instance.UIManager
                .SelectionUI == null)
        {
            return SelectionUIenum.None;
        }

        return
            GameManager.Instance
                .UIManager
                .SelectionUI
                .nowUIenum;
    }

    private CoinType GetNowMoveType()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.InputManager == null)
        {
            return CoinType.None;
        }

        InputManager inputManager =
            GameManager.Instance
                .InputManager;

        // 실제 조작 모드(nowMovingThing)는 유지하되,
        // UI 위에 마우스가 있을 때 CanControl 표시만 None으로 취급한다.
        // 이렇게 해야 UI에서 빠져나왔을 때 World / Player 모드가 그대로 복원된다.
        if (EventSystem.current != null &&
            EventSystem.current
                .IsPointerOverGameObject())
        {
            return CoinType.None;
        }

        // 프로젝트에서 별도로 관리하는 UI 입력 차단 상태도 동일하게 처리.
        if (inputManager.isTouchingUI ||
            inputManager.isSettingUI)
        {
            return CoinType.None;
        }

        return inputManager.nowMovingThing;
    }

    private string GetTranslatedText(
        TranslationData data)
    {
        if (data == null ||
            data.Strings == null)
        {
            return "";
        }

        if (GameManager.Instance == null ||
            GameManager.Instance
                .TranslationManager == null)
        {
            return "";
        }

        LanguageEnum language =
            GameManager.Instance
                .TranslationManager
                .GetLanguage();

        System.Reflection.FieldInfo field =
            typeof(SaveString)
                .GetField(
                    language.ToString()
                );

        if (field == null)
            return "";

        object value =
            field.GetValue(
                data.Strings
            );

        return value != null
            ? value.ToString()
            : "";
    }

    private void ChangeHeight(
        int activeCount)
    {
        if (thisRect == null)
            return;

        float targetHeight =
            activeCount *
            OneKeyHeight;

        thisRect.DOKill();

        thisRect.DOSizeDelta(
            new Vector2(
                thisRect.sizeDelta.x,
                targetHeight
            ),
            ChangeHeightTime
        )
        .SetEase(
            Ease.OutQuad
        );
    }
}
