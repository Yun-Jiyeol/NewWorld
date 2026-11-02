using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartPage_NamePage : NeedToDoOnit
{
    string NowName;

    public List<TextMeshProUGUI> NameSpaces;

    int NameSpaceLength;

    Coroutine nameRoutine;

    [Header("Effect")]
    public float changeEffectTime = 0.6f;
    public float changeEffectInterval = 0.04f;
    public float pageChangeTime = 5f;

    string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_#@";

    private void Awake()
    {
        NameSpaceLength = NameSpaces.Count;
    }

    private void Start()
    {
        Onit();
    }

    public override void Onit()
    {
        if (GameManager.Instance.NowWorldSaveFolder == null)
        {
            NowName = "";
        }
        else
        {
            string saveName;
            string saveTime;
            int iconIndex;
            int tapeIndex;

            if (SaveAndLoadManager.TryParseSaveFolderName(
                GameManager.Instance.NowWorldSaveFolder,
                out saveName,
                out saveTime,
                out iconIndex,
                out tapeIndex))
            {
                NowName = saveName;
            }
            else
            {
                NowName = "";
            }
        }

        ChangeName();
    }

    void ChangeName()
    {
        if (nameRoutine != null)
            StopCoroutine(nameRoutine);

        nameRoutine = StartCoroutine(ChangeNameRoutine());
    }

    IEnumerator ChangeNameRoutine()
    {
        yield return StartCoroutine(NameChangeEffect());

        if (string.IsNullOrEmpty(NowName))
        {
            SetText("");
            yield break;
        }

        int pageIndex = 0;

        while (true)
        {
            int startIndex = pageIndex * NameSpaceLength;

            if (startIndex >= NowName.Length)
            {
                pageIndex = 0;
                startIndex = 0;
            }

            string currentText = "";

            int remainLength = NowName.Length - startIndex;
            int takeLength = Mathf.Min(NameSpaceLength, remainLength);

            currentText = NowName.Substring(startIndex, takeLength);

            SetText(currentText);

            pageIndex++;

            if (NowName.Length <= NameSpaceLength)
                yield break;

            yield return new WaitForSeconds(pageChangeTime);
            yield return StartCoroutine(NameChangeEffect());
        }
    }

    IEnumerator NameChangeEffect()
    {
        float timer = 0f;

        while (timer < changeEffectTime)
        {
            for (int i = 0; i < NameSpaceLength; i++)
            {
                NameSpaces[i].text = GetRandomChar().ToString();
            }

            timer += changeEffectInterval;
            yield return new WaitForSeconds(changeEffectInterval);
        }
    }

    void SetText(string text)
    {
        for (int i = 0; i < NameSpaceLength; i++)
        {
            if (i < text.Length)
                NameSpaces[i].text = text[i].ToString();
            else
                NameSpaces[i].text = "";
        }
    }

    char GetRandomChar()
    {
        int index = Random.Range(0, randomChars.Length);
        return randomChars[index];
    }
}
