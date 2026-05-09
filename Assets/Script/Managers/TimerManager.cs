using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public IEnumerator TimerCorootine(float time, Action _play)
    {
        yield return new WaitForSeconds(time);

        _play?.Invoke();
    }
}
