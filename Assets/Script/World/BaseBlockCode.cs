using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBlockCode : MonoBehaviour
{
    Renderer ren;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        ren = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SettingPosition(float _x, float _y, float _z) 
    {
        gameObject.transform.localPosition = new Vector3(_x, _y, _z);
    }

    public void SetColor(float _alfar)
    {
        ren.GetPropertyBlock(mpb);

        Color baseColor = ren.sharedMaterial.color;
        baseColor.a = _alfar;

        mpb.SetColor("_Color", baseColor);
        ren.SetPropertyBlock(mpb);
    }

    public void SetColor(Color color)
    {
        ren.material.color = color;
    }
}
