using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAction : MonoBehaviour
{
    public RectTransform target;

    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;

    public RectTransform canvasRect;

    float _left;
    float _right;
    float _top;
    float _bottom;

    void Update()
    {
         _left = target.anchoredPosition.x - target.rect.width * 0.5f;
         _right = target.anchoredPosition.x + target.rect.width * 0.5f;
         _top = target.anchoredPosition.y + target.rect.height * 0.5f;
         _bottom = target.anchoredPosition.y - target.rect.height * 0.5f;

        _left = 970f + _left;
        _right = 970f - _right;
        _top = 550f - _top;
        _bottom = 550f + _bottom;

        left.sizeDelta = new Vector2(_left,0);
        right.sizeDelta = new Vector2(_right,0);
        top.sizeDelta = new Vector2(0,_top);
        bottom.sizeDelta = new Vector2(0,_bottom);
    }
}
