using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
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

    public Image background;

    public SoundData ChangValueSound;
    public SoundData ResetSound;

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

        float soundInterval = 0.15f;
        float soundTimer = 0f;

        Transform light = ML.transform;
        Quaternion delta = Quaternion.Euler(
            changVectorrot.x / RollingSpeed,
            changVectorrot.y / RollingSpeed,
            0f
        );

        if (ChangValueSound != null)
        {
            GameManager.Instance.SoundManager.PlayEffect(ChangValueSound);
        }

        while (Input.GetMouseButton(0))
        {
            light.rotation *= delta;

            soundTimer += Time.deltaTime;

            if (soundTimer >= soundInterval)
            {
                soundTimer = 0f;

                if (ChangValueSound != null)
                {
                    GameManager.Instance.SoundManager.PlayEffect(ChangValueSound);
                }
            }

            yield return null;
        }
    }

    public void ChangeColor()
    {
        ML.GetComponent<Light>().color = _color;
    }

    public void ResetMainLight()
    {
        StartCoroutine(ResetRotOfMainLight());
    }

    public void ChangeInlineColor(Color color)
    {
        inLineChoose = color;
        background.color = color;
    }

    IEnumerator ResetRotOfMainLight()
    {
        blockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, gameObject.transform);

        Transform light = ML.transform;

        Quaternion startRot = light.rotation;
        Quaternion endRot = Quaternion.Euler(90f, 0f, 0f);

        float duration = 1f;
        float time = 0f;

        GameManager.Instance.SoundManager.PlayEffect(ResetSound);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            light.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // 마지막 오차 보정
        light.rotation = endRot;

        Destroy(blockImage);
        blockImage = null;
    }
}
