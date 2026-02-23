using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleBlcok : MonoBehaviour
{
    Renderer ren;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        ren = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SetColor(Color color)
    {
        ren.GetPropertyBlock(mpb);

        mpb.SetColor("_Color", color);
        ren.SetPropertyBlock(mpb);
    }
}
