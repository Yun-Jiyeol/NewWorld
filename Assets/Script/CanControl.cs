using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CanControlKeyData
{
    [Header("Action")]
    [Tooltip("KeyManager.KeyboardDictionary에서 가져올 키")]
    public KeyNameSaves Key;

    [Header("Explain")]
    [Tooltip("행동 설명용 TranslationTable")]
    public TranslationData ExplainText;

    [Header("Flow Condition")]
    [Tooltip("비어 있으면 GameFlow 조건을 사용하지 않음")]
    public List<GameFlow> ShowFlows =
        new List<GameFlow>();

    [Header("UI Condition")]
    [Tooltip("비어 있으면 현재 SelectionUI 조건을 사용하지 않음")]
    public List<SelectionUIenum> ShowUIs =
        new List<SelectionUIenum>();

    [Header("Control Condition")]
    [Tooltip("비어 있으면 현재 이동 대상 조건을 사용하지 않음")]
    public List<CoinType> ShowMoveTypes =
        new List<CoinType>();

    public bool CanShow(
        GameFlow flow,
        SelectionUIenum ui,
        CoinType moveType)
    {
        bool flowMatch =
            ShowFlows == null ||
            ShowFlows.Count == 0 ||
            ShowFlows.Contains(flow);

        bool uiMatch =
            ShowUIs == null ||
            ShowUIs.Count == 0 ||
            ShowUIs.Contains(ui);

        bool moveMatch =
            ShowMoveTypes == null ||
            ShowMoveTypes.Count == 0 ||
            ShowMoveTypes.Contains(moveType);

        return flowMatch &&
               uiMatch &&
               moveMatch;
    }
}

public class CanControl : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject KeyEx;

    [Header("Key Explain Data")]
    public List<CanControlKeyData> KeyDatas =
        new List<CanControlKeyData>();

    [Header("Size")]
    public float OneKeyHeight = 30f;
    public float ChangeHeightTime = 0.2f;

    private RectTransform thisRect;

    // 만들어진 KeyEx는 삭제하지 않고 재사용
    private readonly List<KeyEx> keyPool =
        new List<KeyEx>();

    private GameFlow beforeFlow = (GameFlow)(-1);
    private SelectionUIenum beforeUI =
        (SelectionUIenum)(-1);
    private CoinType beforeMoveType =
        (CoinType)(-1);
    private LanguageEnum beforeLanguage =
        (LanguageEnum)(-1);

    private void Awake()
    {
        thisRect = GetComponent<RectTransform>();

        // 이미 자식으로 존재하는 KeyEx도 풀에 포함
        KeyEx[] existingKeys =
            GetComponentsInChildren<KeyEx>(true);

        for (int i = 0; i < existingKeys.Length; i++)
        {
            if (existingKeys[i] == null)
                continue;

            if (!keyPool.Contains(existingKeys[i]))
            {
                keyPool.Add(existingKeys[i]);
                existingKeys[i].gameObject.SetActive(false);
            }
        }
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

        if (GameManager.Instance.TranslationManager != null)
        {
            nowLanguage =
                GameManager.Instance.TranslationManager.GetLanguage();
        }

        // Flow / 현재 UI / 조작 대상 / 언어 중
        // 하나라도 바뀐 순간에만 다시 구성
        if (beforeFlow != nowFlow ||
            beforeUI != nowUI ||
            beforeMoveType != nowMoveType ||
            beforeLanguage != nowLanguage)
        {
            Refresh();
        }
    }

    /// <summary>
    /// 현재 GameFlow와 KeyManager 설정을 기준으로
    /// 표시할 KeyEx를 다시 구성한다.
    /// </summary>
    public void Refresh()
    {
        if (GameManager.Instance == null)
            return;

        KeyManager keyManager =
            GameManager.Instance.KeyManager;

        if (keyManager == null)
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

        if (GameManager.Instance.TranslationManager != null)
        {
            beforeLanguage =
                GameManager.Instance.TranslationManager.GetLanguage();
        }

        ResetAllKeyEx();

        int useCount = 0;

        for (int i = 0; i < KeyDatas.Count; i++)
        {
            CanControlKeyData data =
                KeyDatas[i];

            if (data == null)
                continue;

            if (!data.CanShow(
                nowFlow,
                nowUI,
                nowMoveType))
            {
                continue;
            }

            // 키 자체의 모든 정보는 KeyManager에서 가져온다.
            if (!keyManager.KeyboardDictionary.TryGetValue(
                data.Key,
                out Keyboards keyboard))
            {
                continue;
            }

            if (keyboard == null)
                continue;

            KeyEx keyEx =
                GetKeyEx(useCount);

            if (keyEx == null)
                continue;

            keyEx.gameObject.SetActive(true);
            keyEx.transform.SetSiblingIndex(useCount);

            // 표시 키:
            // KeyManager가 현재 언어/특수키/Shift 조합까지 처리한다.
            string settingKey =
                keyManager.GetKeyFromKeyCode(keyboard);

            // Shift 필요 여부 역시 KeyManager의 데이터 사용
            bool needShift =
                keyboard.shiftClick;

            keyEx.Setting(
                settingKey,
                GetTranslatedText(data.ExplainText),
                needShift
            );

            useCount++;
        }

        ChangeHeight(useCount);
    }

    private SelectionUIenum GetNowUI()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.UIManager == null ||
            GameManager.Instance.UIManager.SelectionUI == null)
        {
            return SelectionUIenum.None;
        }

        return
            GameManager.Instance.UIManager.SelectionUI.nowUIenum;
    }

    private CoinType GetNowMoveType()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.InputManager == null)
        {
            return CoinType.None;
        }

        return
            GameManager.Instance.InputManager.nowMovingThing;
    }

    /// <summary>
    /// 현재 생성되어 있는 KeyEx를 전부 끈다.
    /// Destroy하지 않고 계속 재사용한다.
    /// </summary>
    private void ResetAllKeyEx()
    {
        for (int i = 0; i < keyPool.Count; i++)
        {
            if (keyPool[i] == null)
                continue;

            keyPool[i].ResetKey();
            keyPool[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 필요한 번호의 KeyEx를 가져온다.
    /// 부족할 때만 새로 생성한다.
    /// </summary>
    private KeyEx GetKeyEx(int index)
    {
        while (keyPool.Count <= index)
        {
            if (KeyEx == null)
            {
                Debug.LogWarning(
                    "[CanControl] KeyEx Prefab이 설정되지 않았습니다."
                );
                return null;
            }

            GameObject newKey =
                Instantiate(KeyEx, transform);

            KeyEx newKeyEx =
                newKey.GetComponent<KeyEx>();

            if (newKeyEx == null)
            {
                Debug.LogWarning(
                    "[CanControl] KeyEx Prefab에 KeyEx 컴포넌트가 없습니다."
                );

                Destroy(newKey);
                return null;
            }

            newKey.SetActive(false);
            keyPool.Add(newKeyEx);
        }

        return keyPool[index];
    }

    /// <summary>
    /// TranslationTable에서 현재 언어의 설명을 가져온다.
    /// LanguageEnum 이름과 SaveString 필드 이름을 맞춰 사용한다.
    /// </summary>
    private string GetTranslatedText(
        TranslationData translationData)
    {
        if (translationData == null ||
            translationData.Strings == null)
        {
            return "";
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.TranslationManager == null)
        {
            return "";
        }

        LanguageEnum language =
            GameManager.Instance.TranslationManager.GetLanguage();

        string languageFieldName =
            language.ToString();

        System.Reflection.FieldInfo field =
            typeof(SaveString).GetField(languageFieldName);

        if (field == null)
        {
            Debug.LogWarning(
                "[CanControl] TranslationTable에 현재 언어 필드가 없습니다: "
                + languageFieldName
            );

            return "";
        }

        object value =
            field.GetValue(translationData.Strings);

        return value != null
            ? value.ToString()
            : "";
    }

    private void ChangeHeight(int keyCount)
    {
        if (thisRect == null)
            return;

        float targetHeight =
            keyCount * OneKeyHeight;

        thisRect.DOKill();

        thisRect.DOSizeDelta(
            new Vector2(
                thisRect.sizeDelta.x,
                targetHeight
            ),
            ChangeHeightTime
        ).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// 키 설정 변경 직후 호출하면
    /// 현재 KeyManager 값으로 즉시 다시 표시한다.
    /// </summary>
    public void RefreshKeySetting()
    {
        Refresh();
    }

    public void Clear()
    {
        ResetAllKeyEx();
        ChangeHeight(0);
    }
}
