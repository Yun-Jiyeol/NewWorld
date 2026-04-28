using DG.Tweening;
using System.Collections;
using UnityEngine;

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

    Vector2 screenCenter;
    RectTransform thisTransform;

    Coroutine followCoroutine;
    Vector3 nowPos = Vector3.zero;

    private void Start()
    {
        GameManager.Instance.CursorMover = this;

        thisTransform = GetComponent<RectTransform>();

        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        ChangeCursorState(cursorstate.Follow);
    }

    public void ChangeBeforeState()
    {
        ChangeCursorState(beforestate);
    }

    public void ChangeCursorState(Vector3 _Anchorpos)
    {
        if (nowPos == _Anchorpos) return;

        if(nowstate != cursorstate.OnSomething)
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

        if(followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }

        switch (nowstate)
        {
            case cursorstate.Off:
                SetOff();
                break;
            case cursorstate.Follow:
                SetFollow();
                break;
        }
    }

    void SetOff()
    {
        Vector2 mousePos = Input.mousePosition;

        // 중심 → 마우스 방향 반대
        Vector2 dir = (mousePos - screenCenter).normalized;

        // 반대 방향으로 멀리 이동
        Vector2 targetPos = mousePos + dir * 1000f;

        thisTransform.DOMove(targetPos, 0.5f);
    }

    void SetFollow()
    {
        followCoroutine = StartCoroutine(MoveFollower());
    }

    IEnumerator MoveFollower()
    {
        thisTransform.DOMove(Input.mousePosition, 0.3f);

        yield return new WaitForSeconds(0.3f);

        while (true)
        {
            thisTransform.position = Input.mousePosition;
            yield return null;
        }
    }

    void SetOnSomething(Vector3 _Anchorpos)
    {
        nowPos = _Anchorpos;
        thisTransform.DOAnchorPos(_Anchorpos, 0.3f);
    }
}
