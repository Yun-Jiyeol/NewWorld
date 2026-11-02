using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveName : NeedToDoOnit
{
    string NowName;
    Coroutine _moveCoroutine = null;
    float moveSpeed = 200f;

    private void Start()
    {
        Onit();
    }

    public override void Onit()
    {
        NowName = GameManager.Instance.NowWorldSaveFolder;

        if(NowName == null)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = "";

            if(_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }
        else
        {
            string saveName;
            string saveTime;
            int iconIndex;
            int tapeIndex;

            if (!SaveAndLoadManager.TryParseSaveFolderName(
                NowName,
                out saveName,
                out saveTime,
                out iconIndex,
                out tapeIndex))
            {
                gameObject.GetComponent<TextMeshProUGUI>().text = "";
                return;
            }

            gameObject.GetComponent<TextMeshProUGUI>().text = saveName;

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveCoroutine());
        }
    }

    IEnumerator MoveCoroutine()
    {
        RectTransform thisRect =
            GetComponent<RectTransform>();

        thisRect.anchoredPosition = new Vector2(850f, 0);

        while (true)
        {
            // 시간 기반 이동
            thisRect.anchoredPosition -= new Vector2(moveSpeed * Time.deltaTime, 0);

            // 위치 리셋
            if (thisRect.anchoredPosition.x < -800f)
            {
                thisRect.anchoredPosition =
                    new Vector2(800f, 0f);
            }

            yield return null;
        }
    }
}
