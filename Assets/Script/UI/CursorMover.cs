using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum cursorstate
{
    Off,
    Follow,
    OnSomething
}

public class CursorMover : MonoBehaviour
{
    cursorstate nowstate = cursorstate.Off;
    cursorstate beforestate = cursorstate.Off;

    RectTransform thisTransform;

    Coroutine followCoroutine;
    Vector3 nowPos = Vector3.zero;

    public Sprite InGame;
    public Sprite OutGame;
    Image thisImage;

    [Header("Alpha")]
    [SerializeField] private float visibleAlpha = 200f / 255f;
    [SerializeField] private float hiddenAlpha = 0f;
    [SerializeField] private float fadeTime = 0.2f;

    private void Start()
    {
        GameManager.Instance.CursorMover = this;

        thisTransform = GetComponent<RectTransform>();
        thisImage = GetComponent<Image>();

        SetImageAlpha(hiddenAlpha);

        ChangeCursorState(cursorstate.Follow);
    }

    public void ChangeBeforeState()
    {
        ChangeCursorState(beforestate);
    }

    public void ChangeCursorState(Vector3 _Anchorpos)
    {
        if (nowPos == _Anchorpos) return;

        if (nowstate != cursorstate.OnSomething)
        {
            beforestate = nowstate;
            nowstate = cursorstate.OnSomething;
        }

        thisTransform.DOKill();

        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }

        SetOnSomething(_Anchorpos);
    }

    public void ChangeCursorState(cursorstate _state)
    {
        if (nowstate == _state) return;

        thisTransform.DOKill();

        beforestate = nowstate;
        nowstate = _state;

        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }

        switch (nowstate)
        {
            case cursorstate.Off:
                Cursor.lockState = CursorLockMode.Locked;
                SetOff();
                break;

            case cursorstate.Follow:
                Cursor.lockState = CursorLockMode.Confined;
                SetFollow();
                break;

            case cursorstate.OnSomething:
                break;
        }
    }

    void SetOff()
    {
        nowPos = Vector3.zero;

        if (thisImage != null)
        {
            thisImage.DOKill();
            thisImage.DOFade(hiddenAlpha, fadeTime);
        }
    }

    void SetFollow()
    {
        nowPos = Vector3.zero;

        if (thisImage != null)
        {
            thisImage.DOKill();
            thisImage.DOFade(visibleAlpha, fadeTime);
        }

        followCoroutine = StartCoroutine(MoveFollower());
    }

    IEnumerator MoveFollower()
    {
        while (true)
        {
            thisTransform.position = Input.mousePosition;
            yield return null;
        }
    }

    void SetOnSomething(Vector3 _Anchorpos)
    {
        nowPos = _Anchorpos;

        if (thisImage != null)
        {
            thisImage.DOKill();
            thisImage.DOFade(visibleAlpha, fadeTime);
        }

        thisTransform.DOAnchorPos(_Anchorpos, 0.3f);
    }

    public void MouseSpriteSetting(bool isInGame)
    {
        if (thisImage == null) return;

        if (isInGame)
        {
            thisImage.sprite = InGame;
            thisTransform.pivot = new Vector2(0, 1);
            thisTransform.sizeDelta = new Vector2(50, 50);
        }
        else
        {
            thisImage.sprite = OutGame;
            thisTransform.pivot = new Vector2(0.35f, 1);
            thisTransform.sizeDelta = new Vector2(35, 50);
        }

        SetImageAlphaByState();
    }

    private void SetImageAlphaByState()
    {
        if (thisImage == null) return;

        if (nowstate == cursorstate.Off)
        {
            SetImageAlpha(hiddenAlpha);
        }
        else
        {
            SetImageAlpha(visibleAlpha);
        }
    }

    private void SetImageAlpha(float alpha)
    {
        if (thisImage == null) return;

        Color color = thisImage.color;
        color.a = alpha;
        thisImage.color = color;
    }
}