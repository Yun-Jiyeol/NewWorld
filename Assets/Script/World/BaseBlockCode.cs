using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBlockCode : MonoBehaviour
{
    Renderer ren;
    MaterialPropertyBlock mpb;
    Color OriginalColor;
    Color NowColor;

    void Awake()
    {
        ren = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SettingPosition(float _x, float _y, float _z) 
    {
        gameObject.transform.localPosition = new Vector3(_x, _y, _z);
        OriginalColor = new Color(1,1,1);
    }

    public void SetColor(float _alfar)
    {
        ren.GetPropertyBlock(mpb);

        Color baseColor = ren.sharedMaterial.color;
        baseColor.a = _alfar;
        OriginalColor = baseColor;

        mpb.SetColor("_Color", baseColor);
        ren.SetPropertyBlock(mpb);
    }

    public void SetColor(Color color)
    {
        ren.material.color = color;
        OriginalColor = color;
    }

    public void MakeOriginalColor()
    {
        SetColor(OriginalColor);
    }

    public void AddLight(Color _color)
    {
        if(NowColor ==  null) NowColor = OriginalColor;

        _color.a = 1;

        NowColor += _color;
        ren.material.color = NowColor;
    }
}
