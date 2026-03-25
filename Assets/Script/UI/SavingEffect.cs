using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class SavingEffect : MonoBehaviour
{
    public RectTransform target; // ¿òÁ÷ÀÏ UI

    float floatDuration = 2f;
    float height = 10f;

    float rotateDuration = 0.75f;

    Vector2 startPos;

    void OnEnable()
    {
        startPos = target.anchoredPosition;
        StartCoroutine(Loop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        target.DOKill(); 

        target.anchoredPosition = startPos;
        target.rotation = Quaternion.identity;
    }

    IEnumerator Loop()
    {
        while (true)
        {
            float time = 0f;

            while (time < floatDuration)
            {
                time += Time.deltaTime;
                float t = time / floatDuration;

                float y = Mathf.Sin(t * Mathf.PI * 2f) * height;

                target.anchoredPosition = startPos + new Vector2(0, y);

                yield return null;
            }

            target.anchoredPosition = startPos;

            yield return target
                .DORotate(new Vector3(0, 0, 360f), rotateDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .WaitForCompletion();
        }
    }
}