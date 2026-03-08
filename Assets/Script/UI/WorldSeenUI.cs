using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldSeenUI : MonoBehaviour
{
    public GameObject BG;
    Image BGImage;
    public GameObject SelectWorld;

    Coroutine NowCorountine;

    private void Awake()
    {
        BGImage = BG.GetComponent<Image>();
    }

    public void OnScreen()
    {
        gameObject.SetActive(true);
        BG.SetActive(true);
        SelectWorld.SetActive(true);

        BGImage.color = new Color(0, 0, 0, 0);

        if(NowCorountine != null) StopCoroutine(NowCorountine);
        NowCorountine = StartCoroutine(ChangeScreenCoroutine(4/5f));
    }

    public void OffScreen()
    {
        if (NowCorountine != null) StopCoroutine(NowCorountine);
        NowCorountine = StartCoroutine(ChangeScreenCoroutine(0f, true));
    }

    IEnumerator ChangeScreenCoroutine(float targetalfar, bool isoff = false)
    {
        float nowalfar = BGImage.color.a;
        Color newColor = BGImage.color;

        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float newAlpha = Mathf.Lerp(nowalfar, targetalfar, t);

            newColor.a = newAlpha;
            BGImage.color = newColor;

            yield return null;
        }

        BGImage.color = new Color(0, 0, 0, targetalfar);

        yield return new WaitForSeconds(5f);

        if (isoff)
        {
            offScreenDirect();
        }
    }

    public void offScreenDirect()
    {
        GameManager.Instance.MainCamera.SubCamera.SetActive(false);
        gameObject.SetActive(false);
        BG.SetActive(false);
        SelectWorld.SetActive(false);
    }
}
