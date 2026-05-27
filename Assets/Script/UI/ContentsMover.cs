using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentsMover : MonoBehaviour
{
    public RectTransform viewport;

    [Header("Width")]
    public float minWidth = 200f;
    public float maxWidth = 400f;

    [Header("Effect Range")]
    public float topRange = 200f;
    public float bottomRange = -200f;

    void Update()
    {
        foreach (RectTransform item in transform)
        {
            Vector3 localPos = viewport.InverseTransformPoint(item.position);

            float t = Mathf.InverseLerp(
                topRange,
                bottomRange,
                localPos.y
            );

            t = Mathf.Clamp01(t);

            float targetWidth = Mathf.Lerp(minWidth, maxWidth, t);

            // 현재 RectTransform의 기본 width 기준
            float baseWidth = item.rect.width;

            if (baseWidth <= 0f) continue;

            float scaleX = targetWidth / baseWidth;

            Vector3 scale = item.localScale;
            scale.x = scaleX;
            item.localScale = scale;
        }
    }
}