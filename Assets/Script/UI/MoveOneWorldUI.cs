using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
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
            if (target != null && !EventSystem.current.IsPointerOverGameObject())
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
        target.GetComponent<ChunkCode>().ChooseThisWorld();
        GameManager.Instance.InputManager.ChooseWorld();
        GameManager.Instance.ChangeFlow(GameFlow.WatchOneWorld);

        target = null;
    }

    public void MoveOneOriginalPosition()
    {
        target.GetComponent<ChunkCode>().StartChangeEveryThingSmooth(0.5f,true, SaveScale,true, SaveVector3,true, SaveRotation);
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
        GameManager.Instance.WorldManager.DestroyWorld(target);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
        target = null;
    }

    public void CopyWorld()
    {
        target.GetComponent<ChunkCode>().StartChangeEveryThingSmooth(0.5f, true, SaveScale, true, SaveVector3, true, SaveRotation);

        GameObject newCopied = GameManager.Instance.WorldManager.MakeNewWorld(0, 0, 0);
        newCopied.transform.position = target.transform.position;
        newCopied.transform.rotation = target.transform.rotation;
        newCopied.transform.localScale = target.transform.localScale;

        var targetChunk = target.GetComponent<ChunkCode>();
        var newChunk = newCopied.GetComponent<ChunkCode>();

        foreach (var blocks in targetChunk.BlocksinChunk.Values)
        {
            foreach(BlockInChunkData block in blocks.Values)
            {
                newChunk.MakeWorldQueue.Enqueue(new BlockInChunkData{ _color = block._color, _position = block._position});
            }
        }

        inputManager.TargetGameObject = newCopied;

        GameObject blockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, gameObject.transform);
        GameManager.Instance.UIManager.SelectionUI.AdditionalUI.Add(blockImage);

        newChunk.StartBuildRoutine();
    }
}
