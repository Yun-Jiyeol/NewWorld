using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.GraphicsBuffer;

public class MainLightUI : BasicUI
{
    public CopyChoose CopyChoose;
    public CopyLine CopyLine;
    Vector2 changVectorrot = Vector2.zero;

    public Color _color;
    public Color inLineChoose;
    GameObject ML = null;

    public override void OnSetting()
    {
        ML = GameManager.Instance.LightManager.MainLight;
        _color = ML.GetComponent<Light>().color;

        CopyLine.Setting(this);
        CopyChoose.Setting(this);
    }

    public void ChangeValueOfMainLightxRot(float x)
    {
        changVectorrot = Vector2.zero;
        changVectorrot.x = x;

        StartCoroutine(ChangeRotOfMainLight());
    }

    public void ChangeValueOfMainLightyRot(float y)
    {
        changVectorrot = Vector2.zero;
        changVectorrot.y = y;

        StartCoroutine(ChangeRotOfMainLight());
    }

    IEnumerator ChangeRotOfMainLight()
    {
        Transform light = ML.transform;
        Quaternion delta =
            Quaternion.Euler(changVectorrot.x, changVectorrot.y, 0f);

        while (Input.GetMouseButton(0))
        {
            light.rotation *= delta;

            yield return null;
        }
    }

    public void ChangeColor()
    {
        ML.GetComponent<Light>().color = _color;
    }
}
