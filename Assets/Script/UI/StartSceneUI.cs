using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI WritingBoard;
    public RectTransform Zoom;
    public GameObject StartAction;
    public Image RedLight;

    public bool isStart = false;

    [Header("Setting")]
    public int maxLineCount = 10;
    float MintypingSpeed = 0.02f;
    float MaxtypingSpeed = 0.05f;

    List<string> texts = new List<string>();

    Coroutine typingCoroutine;

    private void Awake()
    {
        StartAction.SetActive(true);
        Zoom.sizeDelta = new Vector3(0, 0);
        isStart = false;
    }

    private void Start()
    {
        Zoom.DOSizeDelta(new Vector3(400, 400), 3f);

        Vector2 startPos = Zoom.anchoredPosition;

        Zoom.DOAnchorPos(
            startPos + new Vector2(25, 20),
            5f
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    public void ClickStart()
    {
        Zoom.DOKill();
        AddText("시스템 시작");
        RedLight.DOColor(Color.white, 1f).OnComplete(() =>
        {
            Zoom.DOAnchorPos(new Vector2(0, 0), 1.5f);
            Zoom.GetComponent<Image>().DOColor(new Color(1,1,1,0), 2f);
            Zoom.DOSizeDelta(new Vector2(3200, 2600), 2f).OnComplete(() =>
            {
                Destroy(StartAction);
            });
        });
    }

    public void AddText(string _text)
    {
        // 현재 타이핑 중이면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 새 줄을 맨 위에 추가
        texts.Insert(0, "");

        // 최대 줄 수 제한
        if (texts.Count > maxLineCount)
        {
            texts.RemoveAt(texts.Count - 1);
        }

        typingCoroutine = StartCoroutine(TypeText(_text));
    }

    IEnumerator TypeText(string targetText)
    {
        string current = "";
        float typingSpeed = 0;

        for (int i = 0; i < targetText.Length; i++)
        {
            typingSpeed = Random.Range(MintypingSpeed, MaxtypingSpeed);

            current += targetText[i];

            // 첫 줄 갱신
            texts[0] = current;

            RefreshBoard();

            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    void RefreshBoard()
    {
        WritingBoard.text = "";

        for (int i = 0; i < texts.Count; i++)
        {
            WritingBoard.text += texts[i];

            if (i != texts.Count - 1)
            {
                WritingBoard.text += "\n";
            }
        }
    }
}
