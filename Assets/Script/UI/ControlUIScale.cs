using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlUIScale : MonoBehaviour
{
    SelectionUI su = null;

    public List<RectTransform> ChangeScalexy;
    public List<RectTransform> ChangeScalex;
    public List<RectTransform> ChangeScaley;
    public List<RectTransform> ChangeScaleMiny;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        su = GameManager.Instance.UIManager.SelectionUI;

        if (su == null) return;

        su.OnChangeUIScale += ChangeSize;
        ChangeSize();
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        su = GameManager.Instance.UIManager.SelectionUI;

        if (su == null) return;

        su.OnChangeUIScale -= ChangeSize;
    }

    void ChangeSize()
    {
        float Ratio = su.scaleratio;

        if(ChangeScalexy.Count != 0)
        {
            ChangeXY(Ratio);
        }

        if (ChangeScalex.Count != 0)
        {
            ChangeX(Ratio);
        }

        if (ChangeScaley.Count != 0)
        {
            ChangeY(Ratio);
        }
    }

    void ChangeXY(float ratio)
    {
        foreach (RectTransform t in ChangeScalexy)
        {
            t.localScale = new Vector3(ratio, ratio, 1);
        }
    }

    void ChangeX(float ratio)
    {
        foreach (RectTransform t in ChangeScalex)
        {
            t.localScale = new Vector3(ratio, 1, 1);
        }
    }

    void ChangeY(float ratio)
    {
        foreach (RectTransform t in ChangeScaley)
        {
            t.localScale = new Vector3(1, ratio, 1);
        }
    }

    void ChangeMinY(float ratio)
    {
        foreach (RectTransform t in ChangeScaleMiny)
        {
            t.localScale = new Vector3(1, 1/ratio, 1);
        }
    }
}
