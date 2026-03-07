using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingUI : MonoBehaviour
{
    public RectTransform Arrow;
    float TargetY = 50;
    float TargetS = 65;

    private void Start()
    {
        GameManager.Instance.UIManager.SelectionUI.AdditionalUI.Add(gameObject);
        StartCoroutine(MovingCoroutine());
    }

    IEnumerator MovingCoroutine()
    {
        Arrow.DOAnchorPosY(TargetY, 0.5f);
        Arrow.DOSizeDelta(new Vector2(TargetS, TargetS), 0.5f);

        yield return new WaitForSeconds(0.5f);

        GameManager.Instance.UIManager.SelectionUI.AdditionalUI.Remove(gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
