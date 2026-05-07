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
            Zoom.GetComponent<Image>().DOColor(new Color(0,0,0,0),1f).SetDelay(1f);

            Zoom.DOSizeDelta(new Vector2(3200, 2600), 2f).OnComplete(() =>
            {
                GameManager.Instance.UIManager.StartSceneUI.isStart = true;
                Destroy(StartAction);
            });
        });
    }

    Queue<string> textQueue = new Queue<string>();
    bool isTyping = false;
    public void AddText(string _text)
    {
        textQueue.Enqueue(_text);

        if (!isTyping)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isTyping = true;

        while (textQueue.Count > 0)
        {
            string nextText = textQueue.Dequeue();

            // 새 줄 추가
            texts.Insert(0, "");

            if (texts.Count > maxLineCount)
            {
                texts.RemoveAt(texts.Count - 1);
            }

            // ⭐ 한 줄 끝날 때까지 기다림
            yield return StartCoroutine(TypeText(nextText, 0));
        }

        isTyping = false;
    }

    IEnumerator TypeText(string targetText, int lineIndex)
    {
        string current = "";

        for (int i = 0; i < targetText.Length; i++)
        {
            float typingSpeed = Random.Range(MintypingSpeed, MaxtypingSpeed);

            current += targetText[i];

            if (lineIndex < texts.Count)
            {
                texts[lineIndex] = current;
            }

            RefreshBoard();

            yield return new WaitForSeconds(typingSpeed);
        }
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
