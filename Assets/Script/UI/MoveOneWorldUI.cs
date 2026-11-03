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

        isWorldMove = true;
        ApplyWorldControlState();

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
        yield return new WaitForSeconds(0.2f);

        while (gameObject.activeSelf)
        {
            if (target != null &&
                EventSystem.current != null &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                if (keyManager.isGetKeyDown(
                    keyManager.KeyboardDictionary[
                        KeyNameSaves.PopUpUI
                    ]))
                {
                    isWorldMove = !isWorldMove;

                    if (isWorldMove)
                    {
                        ApplyWorldControlState();
                    }
                    else
                    {
                        ApplyPlayerControlState();
                    }
                }

                if (isWorldMove)
                {
                    GameManager.Instance.MainCamera.stopCamera = true;
                    inputManager.nowMovingThing = CoinType.World;

                    inputManager.TargetMove(target);
                    inputManager.TargetScale(target);
                    inputManager.TargetTurn(target);
                }
                else
                {
                    GameManager.Instance.MainCamera.stopCamera = false;
                    inputManager.nowMovingThing = CoinType.Player;

                    inputManager.PlayerMove();
                }
            }
            else
            {
                inputManager.nowMovingThing = CoinType.None;
            }

            yield return null;
        }
    }

    void ApplyWorldControlState()
    {
        inputManager.nowMovingThing = CoinType.World;

        GameManager.Instance.CursorMover
            ?.ChangeCursorState(cursorstate.Follow);

        Cursor.lockState =
            CursorLockMode.Confined;

        GameManager.Instance.MainCamera.stopCamera =
            true;
    }

    void ApplyPlayerControlState()
    {
        inputManager.nowMovingThing = CoinType.Player;

        GameManager.Instance.CursorMover
            ?.ChangeCursorState(cursorstate.Off);

        Cursor.lockState =
            CursorLockMode.Locked;

        GameManager.Instance.MainCamera.TryCheckrot();

        GameManager.Instance.MainCamera.stopCamera =
            false;

        GameManager.Instance.MainCamera
            .IgnoreMouseInputForFrames(2);
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
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
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
