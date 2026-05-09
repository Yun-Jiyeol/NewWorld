using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockUI : MonoBehaviour
{
    public RectTransform UpLock;
    public RectTransform DownLock;
    RectTransform thisRec;
    Image UpLockImage;
    Image DownLockImage;

    Coroutine TimerCoroutine = null;

    private void Awake()
    {
        UpLockImage = UpLock.gameObject.GetComponent<Image>();
        DownLockImage = DownLock.gameObject.GetComponent<Image>();
        thisRec = gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        OffthisUI();
    }

    public void OnLock()
    {
        _doKill();
        OnthisUI();

        UpLock.DOAnchorPos(new Vector2(0, 20f), 0.5f);
        UpLockImage.DOColor(Color.white,0.5f).SetDelay(0.2f);
        DownLockImage.DOColor(Color.white, 0.5f).SetDelay(0.2f);

        if (TimerCoroutine != null)
        {
            StopCoroutine(TimerCoroutine);
            TimerCoroutine = null;
        }
    }

    public void OffLock()
    {
        _doKill();

        UpLock.DOAnchorPos(new Vector2(0, 30f), 0.5f);

        timerstart();
    }

    void timerstart()
    {
        if (TimerCoroutine != null)
        {
            StopCoroutine(TimerCoroutine);
        }
        TimerCoroutine = StartCoroutine(GameManager.Instance.TimerManager.TimerCorootine(15f, OffthisUI));
    }

    void OffthisUI()
    {
        thisRec.DOAnchorPosY(-600f, 0.5f);
        TimerCoroutine = null;
    }

    void OnthisUI()
    {
        thisRec.DOAnchorPosY(-487.5f, 0.5f);
    }

    void _doKill()
    {
        thisRec.DOKill();
        UpLock.DOKill();
        DownLock.DOKill();
        UpLockImage.DOKill();
        DownLockImage.DOKill();
    }
}
