using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveOneWorldUI : BasicUI
{
    InputManager inputManager;
    GameObject target;
    Vector3 SaveVector3;
    Quaternion SaveRotation;
    float SaveScale;
    KeyManager keyManager;

    bool isWorldMove = true;

    [Header("SoundData")]
    public SoundData ClickReposSound;
    public SoundData ClickOkaySound;
    public SoundData ClickRebuildSound;
    public SoundData ClickCopySound;
    public SoundData ClickDestroySound;

    private void Awake()
    {
        keyManager = GameManager.Instance.KeyManager;
        inputManager = GameManager.Instance.InputManager;
    }

    public override void OnSetting()
    {
        if (inputManager.TargetGameObject == null) return;
        if (target == null || target != inputManager.TargetGameObject)
        {
            target = inputManager.TargetGameObject;
            SavePos();
        }

        StartCoroutine(updateCoroutine());
    }

    void SavePos()
    {
        SaveVector3 = target.transform.position;
        SaveRotation = target.transform.rotation;
        SaveScale = target.transform.localScale.x;
    }

    IEnumerator updateCoroutine()
    {
        isWorldMove = true;
        yield return new WaitForSeconds(0.2f);

        while (gameObject.activeSelf)
        {
            if (target != null && !EventSystem.current.IsPointerOverGameObject())
            {
                if (keyManager.isGetKeyDown(keyManager.KeyboardDictionary[KeyNameSaves.PopUpUI]))
                {
                    isWorldMove = !isWorldMove;
                }

                if (isWorldMove)
                {
                    inputManager.TargetMove(target);
                    inputManager.TargetScale(target);
                    inputManager.TargetTurn(target);
                    inputManager.nowMovingThing = CoinType.World;
                }
                else
                {
                    inputManager.PlayerMove();
                    GameManager.Instance.MainCamera.CameraTurn();
                    inputManager.nowMovingThing = CoinType.Player;
                }
            }
            else
            {
                inputManager.nowMovingThing = CoinType.None;
            }

            yield return null;
        }
    }

    public void ChangeBlockInWorld()
    {
        GameManager.Instance.SoundManager.PlayEffect(ClickRebuildSound);

        target.GetComponent<ChunkCode>().ChooseThisWorld();
        GameManager.Instance.InputManager.ChooseWorld();
        GameManager.Instance.ChangeFlow(GameFlow.WatchOneWorld);

        target = null;
    }

    public void MoveOneOriginalPosition()
    {
        GameManager.Instance.SoundManager.PlayEffect(ClickReposSound);

        target.GetComponent<ChunkCode>().StartChangeEveryThingSmooth(0.5f,true, SaveScale,true, SaveVector3,true, SaveRotation);
        GameManager.Instance.SaveSomeNowWorld();
    }

    public void SetPosition()
    {
        GameManager.Instance.SoundManager.PlayEffect(ClickOkaySound);
        SavePos();
        GameManager.Instance.SaveSomeNowWorld();
    }

    public void DestroyWorld()
    {
        GameManager.Instance.SoundManager.PlayEffect(ClickDestroySound);

        GameManager.Instance.WorldManager.DestroyWorld(target);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
        target = null;
    }

    public void CopyWorld()
    {
        GameManager.Instance.SoundManager.PlayEffect(ClickCopySound);

        target.GetComponent<ChunkCode>().StartChangeEveryThingSmooth(0.5f, true, SaveScale, true, SaveVector3, true, SaveRotation);
        GameManager.Instance.SaveSomeNowWorld();

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

        GameObject blockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, GameManager.Instance.UIManager.SelectionUI.transform);
        GameManager.Instance.UIManager.SelectionUI.AdditionalUI.Add(blockImage);

        newChunk.StartBuildRoutine();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
