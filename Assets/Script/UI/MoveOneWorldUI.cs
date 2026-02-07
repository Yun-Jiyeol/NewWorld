using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOneWorldUI : BasicUI
{
    InputManager inputManager;
    GameObject target;
    Vector3 SaveVector3;
    Quaternion SaveRotation;
    float SaveScale;

    private void Awake()
    {
        inputManager = GameManager.Instance.InputManager;
    }

    public override void OnSetting()
    {
        if (target == null || target != inputManager.TargetGameObject) 
        {
            target = inputManager.TargetGameObject;
            SaveVector3 = target.transform.position;
            SaveRotation = target.transform.rotation;
            SaveScale = target.transform.localScale.x;
        } 
    }

    private void Update()
    {
        if (target == null) return;
        inputManager.TargetMove(target);
        inputManager.TargetScale(target);
        inputManager.TargetTurn(target);
    }

    public void ChangeBlockInWorld()
    {
        inputManager.StartCoroutine(inputManager.MoveBlockOnMe(target, GameFlow.MakeOneWorld));
    }

    public void MoveOneOriginalPosition()
    {
        target.GetComponent<WorldBaseCode>().StartChangeEveryThingSmooth(1f, SaveScale, SaveVector3, SaveRotation);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }

    public void SetPosition()
    {
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }
}
