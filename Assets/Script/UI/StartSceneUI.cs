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
    public Image RedLight;

    [Header("Setting")]
    public int maxLineCount = 10;

    List<string> texts = new List<string>();

    Coroutine typingCoroutine;

    Queue<string> textQueue = new Queue<string>();
    bool isTyping = false;

    [Header("Setting")]
    public List<TranslationData> ShowText;
    Dictionary<string, SaveString> SaveData = new Dictionary<string, SaveString>();

    private void Awake()
    {
        foreach(TranslationData TD in ShowText)
        {
            SaveData.Add(TD.name, TD.Strings);
        }
    }

    public void AddText(string _text)
    {
        // 꺼져있으면 무시
        if (!gameObject.activeInHierarchy)
            return;

        textQueue.Enqueue(_text);

        if (!isTyping)
        {
            typingCoroutine =
                StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isTyping = true;

        while (textQueue.Count > 0)
        {
            // 비활성화되면 종료
            if (!gameObject.activeInHierarchy)
            {
                break;
            }

            string nextText = textQueue.Dequeue();

            texts.Insert(0, "");

            if (texts.Count > maxLineCount)
            {
                texts.RemoveAt(texts.Count - 1);
            }

            switch (GameManager.Instance.TranslationManager.GetLanguage())
            {
                case LanguageEnum.Com:
                    nextText = SaveData[nextText].Com;
                    break;

                case LanguageEnum.Kor:
                    nextText = SaveData[nextText].Kor;
                    break;

                case LanguageEnum.Eng:
                    nextText = SaveData[nextText].Eng;
                    break;
            }

            yield return StartCoroutine(TypeText(nextText, 0));
        }

        isTyping = false;
        typingCoroutine = null;
    }

    IEnumerator TypeText(string targetText, int lineIndex)
    {
        if (string.IsNullOrEmpty(targetText))
            yield break;

        float totalTime = 1f;
        float timer = 0f;
        int lastCount = 0;

        while (timer < totalTime)
        {
            if (!gameObject.activeInHierarchy)
                yield break;

            timer += Time.deltaTime;

            int count = Mathf.FloorToInt((timer / totalTime) * targetText.Length);
            count = Mathf.Clamp(count, 0, targetText.Length);

            if (count != lastCount)
            {
                string current = targetText.Substring(0, count);

                if (lineIndex < texts.Count)
                {
                    texts[lineIndex] = current;
                }

                RefreshBoard();

                lastCount = count;
            }

            yield return null;
        }

        if (lineIndex < texts.Count)
        {
            texts[lineIndex] = targetText;
        }

        RefreshBoard();
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

    private void OnDisable()
    {
        StopAllCoroutines();

        typingCoroutine = null;

        textQueue.Clear();

        texts.Clear();

        WritingBoard.text = "";

        isTyping = false;

        RedLight.DOKill();
    }
}
