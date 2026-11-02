using DG.Tweening;
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
    RawImage SelectWorldImage;

    Coroutine NowCorountine;

    private void Awake()
    {
        BGImage = BG.GetComponent<Image>();
        SelectWorldImage = SelectWorld.GetComponent<RawImage>();
    }

    public void OnScreen()
    {
        gameObject.SetActive(true);
        BG.SetActive(true);
        SelectWorld.SetActive(true);

        BGImage.color = new Color(0, 0, 0, 0);
        SelectWorldImage.DOColor(Color.white, 0f);

        if (NowCorountine != null) StopCoroutine(NowCorountine);
        NowCorountine = StartCoroutine(ChangeScreenCoroutine(9/10f));
    }

    public void OffScreen()
    {
        if (NowCorountine != null) StopCoroutine(NowCorountine);

        SelectWorldImage.DOColor(new Color(1,1,1,0), 0.5f);
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

        yield return new WaitForSeconds(0.1f);

        if (isoff)
        {
            offScreenDirect();
        }
    }


    public void ResetForStartScreen()
    {
        if (NowCorountine != null)
        {
            StopCoroutine(NowCorountine);
            NowCorountine = null;
        }

        if (BGImage == null && BG != null)
            BGImage = BG.GetComponent<Image>();

        if (SelectWorldImage == null && SelectWorld != null)
            SelectWorldImage = SelectWorld.GetComponent<RawImage>();

        if (BGImage != null)
        {
            BGImage.DOKill();
            BGImage.color = new Color(0f, 0f, 0f, 0f);
        }

        if (SelectWorldImage != null)
        {
            SelectWorldImage.DOKill();
            SelectWorldImage.color = new Color(1f, 1f, 1f, 0f);
        }

        if (BG != null)
            BG.SetActive(false);

        if (SelectWorld != null)
            SelectWorld.SetActive(false);

        gameObject.SetActive(false);
    }
    public void offScreenDirect()
    {
        GameManager.Instance.MainCamera.SubCamera.SetActive(false);
        gameObject.SetActive(false);
        BG.SetActive(false);
        SelectWorld.SetActive(false);
    }
}
