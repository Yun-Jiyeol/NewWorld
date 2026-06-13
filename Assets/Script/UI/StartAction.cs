using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartAction : MonoBehaviour
{
    public RectTransform target;

    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;

    public RectTransform canvasRect;

    float _left;
    float _right;
    float _top;
    float _bottom;

    StartSceneUI StartSceneUI;

    private void Awake()
    {
        target.sizeDelta = new Vector3(0, 0);
    }

    private void Start()
    {
        StartSceneUI = GameManager.Instance.UIManager.StartSceneUI;
        StartCoroutine(StartActionRoutine());
    }

    void Update()
    {
         _left = target.anchoredPosition.x - target.rect.width * 0.5f;
         _right = target.anchoredPosition.x + target.rect.width * 0.5f;
         _top = target.anchoredPosition.y + target.rect.height * 0.5f;
         _bottom = target.anchoredPosition.y - target.rect.height * 0.5f;

        _left = 970f + _left;
        _right = 970f - _right;
        _top = 550f - _top;
        _bottom = 550f + _bottom;

        left.sizeDelta = new Vector2(_left,0);
        right.sizeDelta = new Vector2(_right,0);
        top.sizeDelta = new Vector2(0,_top);
        bottom.sizeDelta = new Vector2(0,_bottom);
    }

    IEnumerator StartActionRoutine()
    {
        StartSceneUI.AddText("SystemReady");

        target.DOSizeDelta(new Vector3(100, 100), 2f);

        Vector2 startPos = target.anchoredPosition;

        target.DOAnchorPos(
            startPos + new Vector2(25, 20),
            5f
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);

        yield return new WaitForSeconds(1.5f);

        StartSceneUI.AddText("ClickAnything");
        while (true)
        {
            if (Input.anyKeyDown)
            {
                ClickStart();
                break;
            }

            yield return null;
        }
    }

    public void ClickStart()
    {
        target.DOKill();
        StartSceneUI.AddText("SystemBoot");
        StartSceneUI.RedLight.DOColor(new Color(50 / 255f, 255 / 255f, 50 / 255f), 1f).OnComplete(() =>
        {
            target.DOAnchorPos(new Vector2(0, 0), 1.5f);
            target.GetComponent<Image>().DOColor(new Color(0, 0, 0, 0), 1f).SetDelay(1f);

            target.DOSizeDelta(new Vector2(3200, 2600), 2f).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }
}
