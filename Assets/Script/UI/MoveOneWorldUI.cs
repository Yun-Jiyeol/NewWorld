using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
        if (inputManager.TargetGameObject == null) return;
        if (target == null || target != inputManager.TargetGameObject) 
        {
            target = inputManager.TargetGameObject;
            SaveVector3 = target.transform.position;
            SaveRotation = target.transform.rotation;
            SaveScale = target.transform.localScale.x;
        }

        StartCoroutine(updateCoroutine());
    }

    IEnumerator updateCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        while (gameObject.activeSelf)
        {
            if (target != null)
            {
                inputManager.TargetMove(target);
                inputManager.TargetScale(target);
                inputManager.TargetTurn(target);
            }

            yield return null;
        }
    }

    public void ChangeBlockInWorld()
    {
        inputManager.StartCoroutine(inputManager.MoveBlockOnMe(target, GameFlow.WatchOneWorld));
        target = null;
    }

    public void MoveOneOriginalPosition()
    {
        target.GetComponent<WorldBaseCode>().StartChangeEveryThingSmooth(0.5f, SaveScale, SaveVector3, SaveRotation, true);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
        target = null;
    }

    public void SetPosition()
    {
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
        target = null;
    }

    public void DestroyWorld()
    {
        target.GetComponent<WorldBaseCode>().DestroyAllBlocks(true);
        target = null;
    }
}
