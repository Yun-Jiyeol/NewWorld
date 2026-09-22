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
    int maxLineCount = 14;

    List<string> texts = new List<string>();

    Coroutine typingCoroutine;

    Queue<string> textQueue = new Queue<string>();
    bool isTyping = false;

    [Header("Setting")]
    public List<TranslationData> ShowText;
    Dictionary<string, SaveString> SaveData = new Dictionary<string, SaveString>();

    [Header("Sound")]
    public SoundData WordStartSound;
    public SoundData WordTypeSound;

    [Header("Text Sound Random Thickness")]
    [SerializeField] private float minLineSoundPitch = 0.86f;
    [SerializeField] private float maxLineSoundPitch = 1.08f;

    [SerializeField] private float minLineSoundVolume = 0.42f;
    [SerializeField] private float maxLineSoundVolume = 0.58f;

    private float currentLineSoundPitch = 1f;
    private float currentLineSoundVolume = 0.5f;

    private void Awake()
    {
        foreach (TranslationData TD in ShowText)
        {
            SaveData.Add(TD.name, TD.Strings);
        }
    }

    public void AddText(string _text)
    {
        if (!gameObject.activeInHierarchy)
            return;

        textQueue.Enqueue(_text);

        if (!isTyping)
        {
            typingCoroutine = StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isTyping = true;

        while (textQueue.Count > 0)
        {
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

                case LanguageEnum.Chs:
                    nextText = SaveData[nextText].Chs;
                    break;

                case LanguageEnum.Cht:
                    nextText = SaveData[nextText].Cht;
                    break;

                case LanguageEnum.Jpn:
                    nextText = SaveData[nextText].Jpn;
                    break;
            }

            SetRandomLineSoundThickness();

            GameManager.Instance.SoundManager.PlayEffect(WordStartSound);

            yield return StartCoroutine(TypeText(nextText, 0));
        }

        isTyping = false;
        typingCoroutine = null;
    }

    void SetRandomLineSoundThickness()
    {
        currentLineSoundPitch = Random.Range(minLineSoundPitch, maxLineSoundPitch);

        float thickness01 = Mathf.InverseLerp(maxLineSoundPitch, minLineSoundPitch, currentLineSoundPitch);

        currentLineSoundVolume = Mathf.Lerp(
            minLineSoundVolume,
            maxLineSoundVolume,
            thickness01
        );
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

                if (targetText[count - 1] != ' ')
                {
                    GameManager.Instance.SoundManager.PlayEffect(
                        WordTypeSound,
                        currentLineSoundVolume,
                        currentLineSoundPitch
                    );
                }

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