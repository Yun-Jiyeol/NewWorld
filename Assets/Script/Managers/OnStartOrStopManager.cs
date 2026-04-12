using System;
using System.Collections;
using System.Collections.Generic;
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
        if(OnStopGame == null && OnPauseGame == null) return;

        OnStopGame += GameManager.Instance.SaveAndLoadManager.SaveAll;
        OnPauseGame += GameManager.Instance.SaveAndLoadManager.SaveAll;
    }

    public void AddTimeActions(Action _action)
    {
        AfterSomeTime = _action;
        StartTimeCoroutine();
    }

    public void DelectActions()
    {
        if (OnStopGame != null || OnPauseGame != null || AfterSomeTime != null)
        {
            OnStopGame?.Invoke();
            OnPauseGame?.Invoke();
            AfterSomeTime?.Invoke();
        }

        OnStopGame = null;
        OnPauseGame = null;
        AfterSomeTime = null;
    }

    void OnApplicationQuit()
    {
        OnStopGame?.Invoke();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) OnPauseGame?.Invoke();
    }

    void OnAtSomeTime()
    {
        AfterSomeTime?.Invoke();
    }

    void StartTimeCoroutine()
    {
        if(TimeCoroutine != null)
        {
            TimeCoroutine = StartCoroutine(TimeSetCoroutine());
        }
    }

    public void StopTimeCoroutine()
    {
        AfterSomeTime = null;
    }

    IEnumerator TimeSetCoroutine()
    {
        while (AfterSomeTime != null)
        {
            yield return new WaitForSeconds(time);
            OnAtSomeTime();
        }

        TimeCoroutine = null;
    }
}
