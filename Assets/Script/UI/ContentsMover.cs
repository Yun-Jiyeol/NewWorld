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
            Vector3 localPos =
                viewport.InverseTransformPoint(item.position);

            // 범위 기준 보간
            float t = Mathf.InverseLerp(
                topRange,
                bottomRange,
                localPos.y
            );

            t = Mathf.Clamp01(t);

            float width =
                Mathf.Lerp(minWidth, maxWidth, t);

            item.sizeDelta =
                new Vector2(width, item.sizeDelta.y);
        }
    }
}
