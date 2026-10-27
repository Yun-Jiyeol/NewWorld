using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("State ScriptableObjects")]
    [Tooltip("위에서부터 검사해서 가장 먼저 맞는 상태 하나를 사용")]
    public List<CanControlStateData> States =
        new List<CanControlStateData>();

    [Header("Size")]
    public float OneKeyHeight = 30f;
    public float ChangeHeightTime = 0.2f;

    [Header("Limit")]
    public int MaxKeyExCount = 3;

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

        CanControlStateData state =
            FindState(
                nowFlow,
                nowUI,
                nowMoveType
            );

        if (state == null)
        {
            HideUnusedKeyEx(0);
            ChangeHeight(0);
            return;
        }

        ShowState(state);
    }

    private CanControlStateData FindState(
        GameFlow flow,
        SelectionUIenum ui,
        CoinType moveType)
    {
        if (States == null)
            return null;

        for (int i = 0;
             i < States.Count;
             i++)
        {
            CanControlStateData state =
                States[i];

            if (state == null)
                continue;

            if (state.IsMatch(
                flow,
                ui,
                moveType))
            {
                return state;
            }
        }

        return null;
    }

    private void ShowState(
        CanControlStateData state)
    {
        if (state.Rows == null)
        {
            HideUnusedKeyEx(0);
            ChangeHeight(0);
            return;
        }

        int rowCount =
            Mathf.Min(
                state.Rows.Count,
                Mathf.Max(
                    0,
                    MaxKeyExCount
                )
            );

        int activeCount = 0;

        for (int i = 0;
             i < rowCount;
             i++)
        {
            CanControlRowData row =
                state.Rows[i];

            if (row == null)
                continue;

            KeyEx keyEx =
                GetKeyEx(activeCount);

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

        HideUnusedKeyEx(
            activeCount
        );

        ChangeHeight(
            activeCount
        );
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

        return
            GameManager.Instance
                .InputManager
                .nowMovingThing;
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
