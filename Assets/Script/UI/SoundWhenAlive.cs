using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundWhenAlive : MonoBehaviour
{
    public SoundData SoundData;

    private void OnEnable()
    {
        StartCoroutine(ConnectionCoroutine());
    }

    IEnumerator ConnectionCoroutine()
    {
        while (GameManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (GameManager.Instance.SoundManager == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;

        GameManager.Instance.SoundManager.ToggleLoopSound(SoundData, true);
    }

    private void OnDisable()
    {
        GameManager.Instance.SoundManager.ToggleLoopSound(SoundData, false);
    }
}
