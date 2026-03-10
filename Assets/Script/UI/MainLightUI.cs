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

    GameObject blockImage= null;

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
        float RollingSpeed = 3f;

        Transform light = ML.transform;
        Quaternion delta =  Quaternion.Euler(changVectorrot.x/ RollingSpeed, changVectorrot.y/ RollingSpeed, 0f);

        while (Input.GetMouseButton(0))
        {
            light.rotation *= delta;

            yield return null;
        }

        GameManager.Instance.SaveAndLoadManager.SaveData();
    }

    public void ChangeColor()
    {
        ML.GetComponent<Light>().color = _color;

        GameManager.Instance.SaveAndLoadManager.SaveData();
    }

    public void ResetMainLight()
    {
        StartCoroutine(ResetRotOfMainLight());
    }

    IEnumerator ResetRotOfMainLight()
    {
        blockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, gameObject.transform);

        Transform light = ML.transform;

        Quaternion startRot = light.rotation;
        Quaternion endRot = Quaternion.Euler(90f, 0f, 0f);

        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            light.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // 마지막 오차 보정
        light.rotation = endRot;

        GameManager.Instance.SaveAndLoadManager.SaveData();

        Destroy(blockImage);
        blockImage = null;
    }
}
