using System;
using System.Collections.Generic;
using UnityEngine;

public enum CanControlInputType
{
    Keys,
    Range
}

[Serializable]
public class CanControlRowData
{
    [Header("Explain")]
    public TranslationData ExplainText;

    [Header("Input Type")]
    public CanControlInputType InputType =
        CanControlInputType.Keys;

    [Header("Keys")]
    [Tooltip("실제 KeyCode가 아니라 KeyManager의 KeyNameSaves를 저장")]
    public List<KeyNameSaves> Keys =
        new List<KeyNameSaves>();

    [Header("Range")]
    [Tooltip("예: firstsave ~ sixthsave")]
    public KeyNameSaves RangeStart;

    public KeyNameSaves RangeEnd;
}

[CreateAssetMenu(
    fileName = "CanControlState",
    menuName = "CanControl/State"
)]
public class CanControlStateData : ScriptableObject
{
    [Header("State Condition")]
    [Tooltip("비어 있으면 GameFlow 조건 무시")]
    public List<GameFlow> Flows =
        new List<GameFlow>();

    [Tooltip("비어 있으면 SelectionUI 조건 무시")]
    public List<SelectionUIenum> UIs =
        new List<SelectionUIenum>();

    [Tooltip("비어 있으면 Player / World / Light 조건 무시")]
    public List<CoinType> MoveTypes =
        new List<CoinType>();

    [Header("KeyEx Rows - Max 3")]
    [Tooltip("이 상태에서 표시할 KeyEx. 최대 3줄")]
    public List<CanControlRowData> Rows =
        new List<CanControlRowData>();

    public bool IsMatch(
        GameFlow flow,
        SelectionUIenum ui,
        CoinType moveType)
    {
        bool flowMatch =
            Flows == null ||
            Flows.Count == 0 ||
            Flows.Contains(flow);

        bool uiMatch =
            UIs == null ||
            UIs.Count == 0 ||
            UIs.Contains(ui);

        bool moveMatch =
            MoveTypes == null ||
            MoveTypes.Count == 0 ||
            MoveTypes.Contains(moveType);

        return flowMatch &&
               uiMatch &&
               moveMatch;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Rows == null)
            return;

        if (Rows.Count > 3)
        {
            Rows.RemoveRange(
                3,
                Rows.Count - 3
            );
        }
    }
#endif
}
