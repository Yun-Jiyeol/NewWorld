using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetWorldPosition : BasicUI
{
    ChunkCode TargetObject = null;

    [Header("SoundData")]
    public SoundData OnClickDestroyBtn;

    public override void OnSetting()
    {
        if (GameManager.Instance == null) return;
        TargetObject = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>();
    }

    public void ClickSetPosBtn()
    {
        GameManager.Instance.InputManager.PutWorldInSpace();
    }

    public void DestroyBtn()
    {
        GameManager.Instance.SoundManager.PlayEffect(OnClickDestroyBtn);
        TargetObject.DestroyWorld();
    }
}
