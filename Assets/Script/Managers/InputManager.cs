using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class InputManager : MonoBehaviour
{
    Transform _cam;
    KeyManager _keyManager;

    RaycastHit OnRaycast = new RaycastHit();
    bool isSomethingHit = false;

    public GameObject TargetGameObject = null;
    float TargetMoveSensitive = 0f;
    float PlayerMoveSpeed = 0f;
    float accelate = 5f;

    public GameObject ReadyToBuildBlock;
    GameObject ReadyToDestroyBlock;
    Vector3? BeforeDestroyBlockPos;
    Vector3? AfterDestroyBlockPos;
    public GameObject SelectWorldPosition;

    Coroutine BuildCoroutine;
    Coroutine DestroyCoroutine;

    public float rotateSpeed = 5f;
    private bool isRotating = false;
    private bool isRotated = false;
    bool showpreviewblock = true;

    public float scaleSpeed = 0.05f;

    public bool isTouchingUI = false;
    public bool isSettingUI = false;
    public bool isLock = false;
    public CoinType nowMovingThing = CoinType.Player;

    private void Awake()
    {
        _cam = Camera.main.transform;
    }

    private void Start()
    {
        _keyManager = GameManager.Instance.KeyManager;
    }

    private void Update()
    {
        if (isTouchingUI) return;
        if (isSettingUI) return;

        switch (GameManager.Instance.flow)
        {
            case GameFlow.WatchAllWorld:
                if (Cursor.lockState == CursorLockMode.Confined)
                {
                    nowMovingThing = CoinType.None;
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);

                    if (GameManager.Instance.UIManager.isReady)
                    {
                        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.PopUpUI]))//나중 esc로 변경
                        {
                            GameManager.Instance.UIManager.SelectionUI.PopInSettingPanel();
                            GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Off);
                            Cursor.lockState = CursorLockMode.Locked;
                            GameManager.Instance.MainCamera.stopCamera = false;
                            GameManager.Instance.MainCamera.SavePos();
                        }
                    }

                    if (EventSystem.current.IsPointerOverGameObject()) return;
                    if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.SelectBtn]))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 1000f, 1<<6))
                        {
                            // 맞은 물체
                            GameObject hitObject = hit.transform.gameObject;

                            // 태그 확인
                            if (hitObject.CompareTag("World"))
                            {
                                TargetGameObject = hitObject.GetComponentInParent<ChunkCode>().gameObject;

                                _cam.GetComponent<MainCamera>().LookAt(TargetGameObject.transform);

                                GameManager.Instance.ChangeFlow(GameFlow.SelectOneWorld);
                            }
                        }
                    }
                }
                else
                {
                    CheckRaycast(true, 1<<6);
                    PlayerMove();

                    nowMovingThing = CoinType.Player;
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);

                    if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.SpawnWorld]))
                    {
                        GameObject newworld = GameManager.Instance.WorldManager.MakeNewWorld(3, 3, 3);

                        TargetGameObject = newworld;
                        TargetGameObject.GetComponent<ChunkCode>().ChooseThisWorld();
                        GameManager.Instance.ChangeFlow(GameFlow.MakeOneWorld);
                        nowMovingThing = CoinType.World;
                        ChooseWorld();
                    }


                    if (GameManager.Instance.UIManager.isReady)
                    {
                        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.PopUpUI]))//나중 esc로 변경
                        {
                            GameManager.Instance.UIManager.SelectionUI.nowUIenum = SelectionUIenum.MainLight;
                            GameManager.Instance.UIManager.SelectionUI.PopUpSettingPanel();
                            GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Follow);
                            Cursor.lockState = CursorLockMode.Confined;
                            GameManager.Instance.MainCamera.stopCamera = true;
                            GameManager.Instance.MainCamera.SavePos();
                        }
                    }

                    if (!isSomethingHit) return;
                    if (EventSystem.current.IsPointerOverGameObject()) return;
                    if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.SelectBtn]))
                    {
                        if (OnRaycast.transform.CompareTag("World"))
                        {
                            TargetGameObject = OnRaycast.transform.GetComponentInParent<ChunkCode>().gameObject;
                            _cam.GetComponent<MainCamera>().LookAt(TargetGameObject.transform);
                            GameManager.Instance.ChangeFlow(GameFlow.SelectOneWorld);
                        }
                    }
                }
                break;

            case GameFlow.SelectOneWorld:
                GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);

                if (EventSystem.current.IsPointerOverGameObject()) return;
                break;

            case GameFlow.WatchOneWorld:
                CheckRaycast(false, 1<<7);
                SaveAndLoadColor();
                ChangeTypeOfMove();

                if (EventSystem.current.IsPointerOverGameObject())
                {
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(CoinType.None);
                    return;
                }
                else
                {
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);
                }

                if(nowMovingThing == CoinType.World)
                {
                    TargetMove(TargetGameObject);
                    TargetScale(TargetGameObject);
                    TargetTurn(TargetGameObject);
                }
                else if (nowMovingThing == CoinType.Player)
                {
                    PlayerMove();
                    GameManager.Instance.MainCamera.CameraTurn();
                }

                if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.LockSelectionKey]))
                {
                    if (isLock)
                    {
                        GameManager.Instance.UIManager.WhatMoving.LockMotion(false);
                        isLock = false;
                    }
                    else
                    {
                        GameManager.Instance.UIManager.WhatMoving.LockMotion(true);
                        isLock = true;
                        return;
                    }
                }
                if (isLock) return;

                if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.DestroyBlockKey]))
                {
                    TryToDestroy();
                    ResetDestroyRange();
                }

                if (!isSomethingHit)
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                else if (OnRaycast.transform.CompareTag("Block"))
                {
                    SeeChooseBlock();

                    if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.AdditionalSelectBtn])
                        && !_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.BuildCube]))
                    {
                        if (BeforeDestroyBlockPos == null)
                        {
                            SettingDestroyRange();
                        }
                        else
                        {
                            SettingDestroyRangeAfter();
                        }
                    }
                    else if(_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.SelectBtn])
                        && !_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.BuildCube]))
                    {
                        SettingDestroyRange();
                    }

                    if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.BuildCube])
                        && !_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.SelectBtn]))
                    {
                        ResetDestroyRange();
                        SpawnNewBlock();

                        Color addcolor = GameManager.Instance.BuildManager.MainColor.color;
                        addcolor.a = GameManager.Instance.BuildManager.alfar;

                        GameManager.Instance.BuildManager.AddPreColor(addcolor);
                    }

                    if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.ChooseBtn]))
                    {
                        ResetDestroyRange();

                        GameManager.Instance.BuildManager.SetColor(TargetGameObject.GetComponent<ChunkCode>().CheckColor(ReadyToBuildBlock.transform.localPosition));
                    }
                }
                else
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                break;

            case GameFlow.PutWorldInSpace:
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(CoinType.None);
                    return;
                }
                else
                {
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);
                }

                ChangeTypeOfMove();
                GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);
                if (nowMovingThing == CoinType.World)
                {
                    TargetMove(TargetGameObject);
                    TargetScale(TargetGameObject);
                    TargetTurn(TargetGameObject);
                }
                else if (nowMovingThing == CoinType.Player)
                {
                    PlayerMove();
                    GameManager.Instance.MainCamera.CameraTurn();
                }

                if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.SelectBtn])) GameManager.Instance.MainCamera.stopCamera = true;
                if (_keyManager.isGetKeyUp(_keyManager.KeyboardDictionary[KeyNameSaves.SelectBtn])) GameManager.Instance.MainCamera.stopCamera = false;
                if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.ChooseBtn]))
                {   
                    Ray ray = _cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 1000f, 1 << 6))
                    {
                        ChunkCode _chunk = TargetGameObject.GetComponent<ChunkCode>();

                        GameObject farthestChunk = null;

                        float farthestValue = float.MinValue;

                        // 카메라 방향
                        Vector3 camDir = _cam.transform.forward.normalized;

                        foreach (var chunk in _chunk.Chunks)
                        {
                            Vector3 worldPos = chunk.Value.transform.position;

                            // 방향 기준 거리 계산
                            float dot = Vector3.Dot(worldPos, camDir);

                            if (dot > farthestValue)
                            {
                                farthestValue = dot;
                                farthestChunk = chunk.Value;
                            }
                        }

                        MeshFilter mf = farthestChunk.GetComponent<MeshFilter>();

                        Mesh mesh = mf.sharedMesh;

                        Vector3 farthestPoint = Vector3.zero;

                        farthestValue = float.MinValue;

                        foreach (Vector3 v in mesh.vertices)
                        {
                            Vector3 worldPos = farthestChunk.transform.TransformPoint(v);

                            float dot = Vector3.Dot(worldPos, camDir);

                            if (dot > farthestValue)
                            {
                                farthestValue = dot;
                                farthestPoint = worldPos;
                            }
                        }

                        Vector3 offset = hit.point - farthestPoint;

                        TargetGameObject.transform.position += offset;
                    }

                    return;
                }
                if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.BuildCube]))
                {
                    GameManager.Instance.ChangeFlow(GameFlow.None);

                    TargetGameObject.transform.parent = GameManager.Instance.WorldManager.SpawnedWorlds.transform;

                    TargetGameObject.GetComponent<ChunkCode>().PutThisWorld();

                    UnChooseWorld();

                    ChangeToWatchAllWorld();
                }
                break;

            case GameFlow.MakeOneWorld:
                CheckRaycast(false, 1<<7);
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(CoinType.None);
                    return;
                }
                else
                {
                    GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);
                }
                TargetScale(TargetGameObject);
                TargetTurn(TargetGameObject);

                GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(nowMovingThing);


                if (!isSomethingHit)
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                else if (OnRaycast.transform.CompareTag("Block"))
                {
                    SeeChooseBlock();
                }
                else
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                break;
        }
    }

    void ChangeTypeOfMove()
    {
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.PopUpUI]))
        {
            if (nowMovingThing == CoinType.None) return;
            else if (nowMovingThing == CoinType.World)
            {
                GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Off);
                Cursor.lockState = CursorLockMode.Locked;
                GameManager.Instance.MainCamera.TryCheckrot();
                nowMovingThing = CoinType.Player;
            }
            else if (nowMovingThing == CoinType.Player)
            {
                GameManager.Instance.CursorMover.ChangeCursorState(cursorstate.Follow);
                Cursor.lockState = CursorLockMode.Confined;
                nowMovingThing = CoinType.World;
            }
        }
    }

    public void WhenComeintoWatchOneWorld()
    {
        ReadyToDestroyBlock = null;
        BeforeDestroyBlockPos = null;
        AfterDestroyBlockPos = null;
    }

    void ResetDestroyRange()
    {
        if(ReadyToDestroyBlock != null) ReadyToDestroyBlock.SetActive(false);
        BeforeDestroyBlockPos = null;
        AfterDestroyBlockPos = null;
    }

    void SettingDestroyRange()
    {
        if (ReadyToDestroyBlock == null)
        {
            ReadyToDestroyBlock = Instantiate(ReadyToBuildBlock);
            ReadyToDestroyBlock.transform.SetParent(
                ReadyToBuildBlock.transform.parent,
                true
            );
            ReadyToDestroyBlock.GetComponent<SampleBlcok>().SetColor(new Color(0.8f,0,0,0.7f));
            ReadyToDestroyBlock.layer = 7;
        }
        ReadyToDestroyBlock.SetActive(true);
        ReadyToDestroyBlock.transform.position = ReadyToBuildBlock.transform.position;
        ReadyToDestroyBlock.transform.localScale = ReadyToBuildBlock.transform.localScale;
        ReadyToDestroyBlock.transform.rotation = ReadyToBuildBlock.transform.rotation;

        BeforeDestroyBlockPos = ReadyToBuildBlock.transform.localPosition;
        AfterDestroyBlockPos = null;
    }

    void SettingDestroyRangeAfter()
    {
        Vector3 NowVector = ReadyToBuildBlock.transform.localPosition;

        if (AfterDestroyBlockPos != null)
        {
            float beforeDist = Vector3.Distance(NowVector, BeforeDestroyBlockPos.Value);
            float afterDist = Vector3.Distance(NowVector, AfterDestroyBlockPos.Value);

            if (afterDist > beforeDist)
            {
                BeforeDestroyBlockPos = AfterDestroyBlockPos.Value;
            }
        }

        AfterDestroyBlockPos = NowVector;
        if (AfterDestroyBlockPos == BeforeDestroyBlockPos)
        {
            AfterDestroyBlockPos = null;
            return;
        }

        Vector3 A = BeforeDestroyBlockPos.Value;
        Vector3 B = AfterDestroyBlockPos.Value;
        Vector3 center = (A + B) / 2f;
        Vector3 size = new Vector3(
        Mathf.Abs(B.x - A.x),
        Mathf.Abs(B.y - A.y),
        Mathf.Abs(B.z - A.z)
         );

        size += Vector3.one * 1.05f;

        ReadyToDestroyBlock.transform.localPosition = center;
        ReadyToDestroyBlock.transform.localScale = size;
        ReadyToDestroyBlock.transform.rotation = ReadyToBuildBlock.transform.rotation;
    }

    void TryToDestroy()
    {
        if (BeforeDestroyBlockPos == null) return;
        if(AfterDestroyBlockPos == null)
        {
            TargetGameObject.GetComponent<ChunkCode>().DestroyBlockData(BeforeDestroyBlockPos.Value);
        }
        else
        {
            List<Vector3> saveddestroypos = new List<Vector3>();
            Vector3 A = BeforeDestroyBlockPos.Value;
            Vector3 B = AfterDestroyBlockPos.Value;

            int minX = Mathf.Min(Mathf.RoundToInt(A.x), Mathf.RoundToInt(B.x));
            int maxX = Mathf.Max(Mathf.RoundToInt(A.x), Mathf.RoundToInt(B.x));

            int minY = Mathf.Min(Mathf.RoundToInt(A.y), Mathf.RoundToInt(B.y));
            int maxY = Mathf.Max(Mathf.RoundToInt(A.y), Mathf.RoundToInt(B.y));

            int minZ = Mathf.Min(Mathf.RoundToInt(A.z), Mathf.RoundToInt(B.z));
            int maxZ = Mathf.Max(Mathf.RoundToInt(A.z), Mathf.RoundToInt(B.z));

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        saveddestroypos.Add(new Vector3Int(x, y, z));
                    }
                }
            }

            TargetGameObject.GetComponent<ChunkCode>().DestroyBlockData(saveddestroypos);
        }
    }

    void ChangeToPutWorldInSpace()
    {
        GameManager.Instance.ChangeFlow(GameFlow.PutWorldInSpace);
    }

    void ChangeToWatchAllWorld()
    {
        GameManager.Instance.MainCamera.GetComponent<Camera>().cullingMask &= ~(1 << 7);

        Destroy(ReadyToBuildBlock);
        ReadyToBuildBlock = null;

        GameManager.Instance.SaveNowWorld();
        TargetGameObject = null;

        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }

    void SeeChooseBlock()
    {
        if (ReadyToBuildBlock == null)
        {
            ReadyToBuildBlock = new GameObject();
            ReadyToBuildBlock.transform.SetParent(TargetGameObject.transform);

            ReadyToBuildBlock.transform.localScale = Vector3.one * 1.03f;
            ReadyToBuildBlock.transform.rotation = new Quaternion(0, 0, 0, 0);
            ReadyToBuildBlock.layer = 7;

            MeshFilter mf = ReadyToBuildBlock.AddComponent<MeshFilter>();
            mf.sharedMesh = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshFilter>().sharedMesh;

            MeshRenderer mr = ReadyToBuildBlock.AddComponent<MeshRenderer>();
            mr.sharedMaterials = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshRenderer>().sharedMaterials;

            ReadyToBuildBlock.AddComponent<SampleBlcok>().SetColor(new Color(0f, 0, 0, 0.7f));
        }
        if (showpreviewblock) ReadyToBuildBlock.SetActive(true);

        Vector3 localHit = TargetGameObject.transform.InverseTransformPoint(OnRaycast.point - OnRaycast.normal * 0.01f);

        Vector3Int grid = Vector3Int.RoundToInt(localHit);

        ReadyToBuildBlock.transform.localPosition = grid;
    }

    void SaveAndLoadColor()
    {
        LoadColor();
        SaveColors();
    }

    void SaveColors()
    {
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.firstsave])) GameManager.Instance.BuildManager.AddSaveColor(0);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.secondsave])) GameManager.Instance.BuildManager.AddSaveColor(1);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.thirdsave])) GameManager.Instance.BuildManager.AddSaveColor(2);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.forthsave])) GameManager.Instance.BuildManager.AddSaveColor(3);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.fifthsave])) GameManager.Instance.BuildManager.AddSaveColor(4);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.sixthsave])) GameManager.Instance.BuildManager.AddSaveColor(5);
    }

    void LoadColor()
    {
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.firstload])) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[0]);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.secondload])) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[1]);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.thirdload])) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[2]);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.forthload])) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[3]);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.fifthload])) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[4]);
        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.sixthload])) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[5]);
    }

    public void TargetMove(GameObject _target, bool isMoveSide = true)
    {
        Vector3 move = Vector3.zero;
        if (isMoveSide)
        {
            if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WUpMove])) move += _cam.up;
            if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WDownMove])) move -= _cam.up;
            if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WRightMove])) move += _cam.right;
            if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WLeftMove])) move -= _cam.right;
        }
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WFrontMove])) move += _cam.forward;
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WBackMove])) move -= _cam.forward;

        if (move != Vector3.zero)
        {
            TargetMoveSensitive += accelate * Time.deltaTime;
            TargetMoveSensitive = Mathf.Clamp(TargetMoveSensitive, 0, TargetMoveSensitive);

            _target.transform.position += move.normalized * TargetMoveSensitive * Time.deltaTime;
        }
        else
        {
            TargetMoveSensitive = 0f;
        }
    }

    public void PlayerMove()
    {
        Vector3 move = Vector3.zero;
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.PFrontMove])) move += _cam.forward;
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.PBackMove])) move -= _cam.forward;
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.PRightMove])) move += _cam.right;
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.PLeftMove])) move -= _cam.right;

        if (move != Vector3.zero)
        {
            PlayerMoveSpeed += accelate * Time.deltaTime;
            PlayerMoveSpeed = Mathf.Clamp(PlayerMoveSpeed, 0, PlayerMoveSpeed);

            _cam.position += move.normalized * PlayerMoveSpeed * Time.deltaTime;
        }
        else
        {
            PlayerMoveSpeed = 0f;
        }
    }

    public void TargetScale(GameObject _target)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) != 0)
        {
            Vector3 scale = _target.transform.localScale;
            scale += Vector3.one * scroll * scaleSpeed;

            _target.transform.localScale = Vector3.one * scale.x;
        }
    }

    public void TargetTurn(GameObject _target)
    {
        isRotating = false;

        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.WorldTurnTrigger]))
        {
            TurnObjectDirect(_target);
            return;
        }

        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.PWBaseMoveKey]))
        {
            isRotating = true;
            isRotated = false;
        }

        if (isRotating && _target != null)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Transform cam = Camera.main.transform;

            if (Mathf.Abs(mouseX) >= 0.2f ||
                Mathf.Abs(mouseY) >= 0.2f)
            {
                isRotated = true;
            }

            // 마우스 방향 기반 회전축
            Vector3 rotationAxis =
                (cam.up * -mouseX) +
                (cam.right * mouseY);

            // 회전량
            float rotationAmount =
                new Vector2(mouseX, mouseY).magnitude
                * rotateSpeed;

            // 하나의 축으로 회전
            _target.transform.Rotate(
                rotationAxis,
                rotationAmount,
                Space.World
            );
        }
    }

    private float rotateTimer = 0f;
    private float rotateDelay = 0.02f;

    void TurnObjectDirect(GameObject _target)
    {
        rotateTimer += Time.deltaTime;

        if (rotateTimer < rotateDelay)
            return;

        rotateTimer = 0f;

        // X
        if (_keyManager.isGetKey(
            _keyManager.KeyboardDictionary[KeyNameSaves.TurnXPlus]))
        {
            _target.transform.Rotate(
                1f,
                0f,
                0f,
                Space.Self
            );
        }
        else if (_keyManager.isGetKey(
            _keyManager.KeyboardDictionary[KeyNameSaves.TurnXMinus]))
        {
            _target.transform.Rotate(
                -1f,
                0f,
                0f,
                Space.Self
            );
        }

        // Y
        if (_keyManager.isGetKey(
            _keyManager.KeyboardDictionary[KeyNameSaves.TurnYPlus]))
        {
            _target.transform.Rotate(
                0f,
                1f,
                0f,
                Space.Self
            );
        }
        else if (_keyManager.isGetKey(
            _keyManager.KeyboardDictionary[KeyNameSaves.TurnYMinus]))
        {
            _target.transform.Rotate(
                0f,
                -1f,
                0f,
                Space.Self
            );
        }

        // Z
        if (_keyManager.isGetKey(
            _keyManager.KeyboardDictionary[KeyNameSaves.TurnZPlus]))
        {
            _target.transform.Rotate(
                0f,
                0f,
                1f,
                Space.Self
            );
        }
        else if (_keyManager.isGetKey(
            _keyManager.KeyboardDictionary[KeyNameSaves.TurnZMinus]))
        {
            _target.transform.Rotate(
                0f,
                0f,
                -1f,
                Space.Self
            );
        }
    }

    void CheckRaycast(bool isCamera, int _layer)
    {
        Ray ray = new Ray();
        if (isCamera) ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        else ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit; 
        
        if (Physics.Raycast(ray, out hit, 1000f, _layer))
        {
            OnRaycast = hit;
            isSomethingHit= true;
        }
        else
        {
            isSomethingHit = false;
        }
    }

    public IEnumerator MoveBlockOnMe(GameObject _targetobject, GameFlow _gameflow)
    {
        GameManager.Instance.ChangeFlow(GameFlow.None);

        TargetGameObject = _targetobject;

        Transform cam = Camera.main.transform;

        float baseDistance = 2f; // 최소 거리

        Vector3 targetPos = cam.position + cam.forward * (baseDistance);

        // 최종 위치 계산 (한 번만)
        Vector3 startPos = TargetGameObject.transform.position;
        Vector3 endPos = targetPos;

        float duration = 0.5f;
        float time = 0f;


        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            TargetGameObject.transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        // 오차 보정
        SelectWorldPosition.transform.position = endPos;
        TargetGameObject.transform.SetParent(SelectWorldPosition.transform);
        TargetGameObject.transform.position = endPos;

        TargetGameObject.GetComponent<ChunkCode>().ChooseThisWorld();
        GameManager.Instance.ChangeFlow(_gameflow);
    }

    /* IEnumerator BuildMoreBlocksOnce() 여러개 설치 코루틴 봉인
    {
        SpawnNewBlock();

        yield return new WaitForSeconds(1f);

        while (ReadyToBuildBlock.activeSelf)
        {
            SpawnNewBlock();
            yield return new WaitForSeconds(0.1f);
        }
    } */

    //BlockInChunkData block = null;

    /*
    IEnumerator DestroyThisBlock()
    {
        block = TargetGameObject.GetComponent<ChunkCode>().DestroyBlockData(ReadyToBuildBlock.transform.localPosition);
        if (block == null) yield break;
        Vector3 normal = OnRaycast.normal.normalized;

        Quaternion startRotationInDestroy = block._go.transform.localRotation;

        float duration = 0.3f;
        float frequency = 7f;   // 흔들리는 속도
        float angle = 25f;      // 고개 꺾이는 각도

        float time = 0f;

        showpreviewblock = false;
        ReadyToBuildBlock.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // -angle ~ +angle
            float swing = Mathf.Sin(time * frequency) * angle * (1f - t);

            block._go.transform.localRotation = startRotationInDestroy * Quaternion.AngleAxis(swing, normal);

            yield return null;
        }

        Destroy(block._go);
        block = null;
        showpreviewblock = true;
    }*/

    void SpawnNewBlock()
    {
        Color SpawnedColor = GameManager.Instance.BuildManager.MainColor.color;
        float alfar = GameManager.Instance.BuildManager.alfar;
        SpawnedColor.a = alfar;

        GameManager.Instance.BuildManager.RequestBuild
            (new WorldAndBlock 
            { world = TargetGameObject.GetComponent<ChunkCode>(), 
            blockposition = TargetGameObject.transform.InverseTransformPoint(OnRaycast.point + OnRaycast.normal * 0.01f),
            blockcolor = SpawnedColor});
    }

    public void ChooseWorld()
    {
        TargetGameObject.GetComponent<ChunkCode>().ChangeWorldLayer(7);
        GameManager.Instance.UIManager.WorldSeenUI.OnScreen();
    }

    public void UnChooseWorld()
    {
        TargetGameObject.GetComponent<ChunkCode>().ChangeWorldLayer(6);
    }

    public void PutThisWorldInSpace()
    {
        if (ReadyToBuildBlock != null)
        {
            Destroy(ReadyToBuildBlock);
        }
        if (ReadyToDestroyBlock != null)
        {
            Destroy(ReadyToDestroyBlock);
            ReadyToDestroyBlock = null;
        }

        GameManager.Instance.ChangeFlow(GameFlow.None);

        GameObject cam = GameManager.Instance.MainCamera.gameObject;

        TargetGameObject.transform.SetParent(cam.transform);
        GameManager.Instance.MainCamera.GetComponent<Camera>().cullingMask |= (1 << 7);
        GameManager.Instance.UIManager.WorldSeenUI.OffScreen();
        Invoke("ChangeToPutWorldInSpace", 0.7f); 
    }
}
