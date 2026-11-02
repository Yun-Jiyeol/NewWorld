using System;
using System.Collections;
using UnityEngine;

public class OnStartOrStopManager : MonoBehaviour
{
    public Action OnStopGame;
    public Action OnPauseGame;
    public Action AfterSomeTime;

    Coroutine TimeCoroutine;
    float time = 600f;

    public void AddActions()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.SaveAndLoadManager == null)
        {
            return;
        }

        Action saveAll =
            GameManager.Instance.SaveAndLoadManager.SaveAll;

        OnStopGame -= saveAll;
        OnStopGame += saveAll;

        OnPauseGame -= saveAll;
        OnPauseGame += saveAll;
    }

    public void AddTimeActions(Action _action)
    {
        AfterSomeTime = _action;
        StartTimeCoroutine();
    }

    public void DelectActions()
    {
        // Flow를 Start로 바꿀 때 저장 콜백을 "실행"하지 않고
        // 등록만 해제한다.
        // 실제 저장은 GoToStartPage / ExitGame / Pause / Quit에서 처리한다.
        OnStopGame = null;
        OnPauseGame = null;
        StopTimeCoroutine();
    }

    void OnApplicationQuit()
    {
        OnStopGame?.Invoke();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            OnPauseGame?.Invoke();
    }

    void OnAtSomeTime()
    {
        AfterSomeTime?.Invoke();
    }

    void StartTimeCoroutine()
    {
        if (TimeCoroutine == null &&
            AfterSomeTime != null)
        {
            TimeCoroutine =
                StartCoroutine(TimeSetCoroutine());
        }
    }

    public void StopTimeCoroutine()
    {
        AfterSomeTime = null;

        if (TimeCoroutine != null)
        {
            StopCoroutine(TimeCoroutine);
            TimeCoroutine = null;
        }
    }

    IEnumerator TimeSetCoroutine()
    {
        while (AfterSomeTime != null)
        {
            yield return new WaitForSeconds(time);

            if (AfterSomeTime == null)
                break;

            OnAtSomeTime();
        }

        TimeCoroutine = null;
    }
}
