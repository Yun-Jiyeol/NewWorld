using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnInputFieldEnter : MonoBehaviour
{
    UI_BtnEffect thisBtnEffect;
    bool BtnEffect = true;

    TMP_InputField thisInput;
    CursorMover cursorMover;
    Coroutine FollowCoroutine = null;

    private void Awake()
    {
        if (!TryGetComponent<TMP_InputField>(out thisInput))
        {
            enabled = false;
        }

        if (!TryGetComponent<UI_BtnEffect>(out thisBtnEffect))
        {
            BtnEffect = false;
        }
    }

    private void Start()
    {
        cursorMover = GameManager.Instance.CursorMover;

        thisInput.onSelect.AddListener(OnSelected);
        thisInput.onEndEdit.AddListener(OnEndChange);
    }

    void OnSelected(string value)
    {
        if (FollowCoroutine != null) return;
        if (BtnEffect) thisBtnEffect.HoldChange();
        FollowCoroutine = StartCoroutine(FollowCursor());
    }

    IEnumerator FollowCursor()
    {
        TMP_Text textComponent = thisInput.textComponent;
        RectTransform textRect = textComponent.rectTransform;
        RectTransform targetRect = cursorMover.GetComponent<RectTransform>();

        Cursor.lockState = CursorLockMode.Locked;

        while (true)
        {
            int caretIndex = thisInput.caretPosition;

            textComponent.ForceMeshUpdate();
            var textInfo = textComponent.textInfo;

            Vector3 localPos;

            if (caretIndex == 0)
            {
                if (textInfo.characterCount > 0)
                {
                    var firstChar = textInfo.characterInfo[0];
                    localPos = firstChar.bottomRight;
                }
                else
                {
                    // 텍스트가 아예 없을 때 (텍스트 시작점 기준)
                    localPos = Vector3.zero;
                }
            }
            else
            {
                if (caretIndex > textInfo.characterCount)
                    caretIndex = textInfo.characterCount;

                var charInfo = textInfo.characterInfo[caretIndex - 1];
                localPos = charInfo.bottomRight;
            }

            Vector3 worldPos = textRect.TransformPoint(localPos);
            Vector3 finalLocalPos = targetRect.parent.InverseTransformPoint(worldPos);

            cursorMover.ChangeCursorState(finalLocalPos);

            yield return null;
        }
    }

    void OnEndChange(string value)
    {
        Cursor.lockState = CursorLockMode.Confined;
        if (BtnEffect) thisBtnEffect.UnHoldChange();

        if (FollowCoroutine != null)
        {
            StopCoroutine(FollowCoroutine);
            FollowCoroutine= null;
        }
        cursorMover.ChangeBeforeState();
    }
}
