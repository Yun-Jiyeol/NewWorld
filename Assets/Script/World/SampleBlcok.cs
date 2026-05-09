using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleBlcok : MonoBehaviour
{
    Renderer ren;
    MaterialPropertyBlock mpb;
    Coroutine thisColorCRT;

    Color StartColor;

    void Awake()
    {
        ren = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void Start()
    {
        ren.GetPropertyBlock(mpb);
        Color startColor = mpb.GetColor("_Color");
        StartColor = startColor;
        startColor.a = 1f;

        if (startColor == Color.black)
            thisColorCRT = StartCoroutine(ColorValueCoroutine());
        else
            thisColorCRT = StartCoroutine(ColorAlfarCoroutine());
    }

    void OnEnable()
    {
        Color checkColor = StartColor;
        checkColor.a = 1f;

        if (checkColor == Color.black)
            thisColorCRT = StartCoroutine(ColorValueCoroutine());
        else
            thisColorCRT = StartCoroutine(ColorAlfarCoroutine());
    }

    void OnDisable()
    {
        if (thisColorCRT != null)
            StopCoroutine(thisColorCRT);
    }

    IEnumerator ColorAlfarCoroutine()
    {
        ren.GetPropertyBlock(mpb);
        Color startColor = mpb.GetColor("_Color");

        float startAlpha = StartColor.a;
        float minAlpha = 0.5f;
        float speed = 1.5f;

        while (true)
        {
            float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;

            float a = Mathf.Lerp(minAlpha, startAlpha, t);

            Color c = startColor;
            c.a = a;

            mpb.SetColor("_Color", c);
            ren.SetPropertyBlock(mpb);

            yield return null;
        }
    }

    IEnumerator ColorValueCoroutine()
    {
        ren.GetPropertyBlock(mpb);
        Color startColor = StartColor;

        float speed = 1f;

        while (true)
        {
            float v = Mathf.PingPong(Time.time * speed, 1f);

            Color c = startColor * (0.5f + v * 0.5f);

            mpb.SetColor("_Color", c);
            ren.SetPropertyBlock(mpb);

            yield return null;
        }
    }

    public void SetColor(Color color)
    {
        ren.GetPropertyBlock(mpb);

        mpb.SetColor("_Color", color);
        ren.SetPropertyBlock(mpb);
    }
}
