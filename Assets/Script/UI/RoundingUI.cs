using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundingUI : MonoBehaviour
{
    private Tween rotateTween;

    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        rotateTween = transform
            .DOLocalRotate(new Vector3(0, 0, 360f), 4f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnDisable()
    {
        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();
    }
}