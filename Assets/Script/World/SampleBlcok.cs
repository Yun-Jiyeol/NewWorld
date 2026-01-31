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

    private void Start()
    {
        Color _rcoloc = new Color(0f, 0f, 0f, 0.4f);
        SetColor(_rcoloc);
    }

    public void SetColor(Color color)
    {
        ren.GetPropertyBlock(mpb);

        mpb.SetColor("_Color", color);
        ren.SetPropertyBlock(mpb);
    }
}
