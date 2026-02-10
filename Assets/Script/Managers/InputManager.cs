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
    public KeyCode MoveUp = KeyCode.W;
    public KeyCode MoveDown = KeyCode.S;
    public KeyCode MoveLeft = KeyCode.A;
    public KeyCode MoveRight = KeyCode.D;
    public KeyCode MoveFar = KeyCode.F;
    public KeyCode MoveClose = KeyCode.Z;

    public KeyCode BuildNewWorld = KeyCode.P;

    RaycastHit OnRaycast = new RaycastHit();
    bool isSomethingHit = false;

    public GameObject TargetGameObject = null;
    public float TargetMoveSensitive = 1f;

    public GameObject ReadyToBuildBlock;
    public GameObject SelectWorldPosition;

    Coroutine BuildCoroutine;
    Coroutine DestroyCoroutine;

    public float rotateSpeed = 5f;
    private bool isRotating = false;
    private bool isRotated = false;
    bool showpreviewblock = true;

    public float scaleSpeed = 0.05f;

    public bool isTouchingUI = false;

    private void Update()
    {
        if(isTouchingUI) return;

        switch (GameManager.Instance.flow)
        {
            case GameFlow.WatchAllWorld:
                CheckRaycast(true);

                if (Input.GetKeyDown(BuildNewWorld))
                {
                    GameManager.Instance.SaveAndLoadManager.SaveData();

                    GameObject newworld = GameManager.Instance.WorldManager.MakeNewWorld(3, 3, 3);
                    StartCoroutine(MoveBlockOnMe(newworld, GameFlow.MakeOneWorld));
                }

                if (!isSomethingHit) return;
                if (Input.GetMouseButtonDown(0))
                {
                    GameManager.Instance.SaveAndLoadManager.SaveData();

                    if (OnRaycast.transform.CompareTag("World"))
                    {
                        TargetGameObject = OnRaycast.transform.gameObject;
                        Camera.main.transform.LookAt(TargetGameObject.GetComponent<BoxCollider>().bounds.center);
                        GameManager.Instance.ChangeFlow(GameFlow.SelectOneWorld);
                    }
                }
                break;

            case GameFlow.SelectOneWorld:
                if (EventSystem.current.IsPointerOverGameObject()) return;
                break;

            case GameFlow.WatchOneWorld:
                //임시 코드
                if (Input.GetKeyDown(KeyCode.P))
                {
                    GameManager.Instance.SaveAndLoadManager.SaveData();

                    if (ReadyToBuildBlock != null)
                    {
                        Destroy(ReadyToBuildBlock);
                    }

                    GameManager.Instance.ChangeFlow(GameFlow.None);

                    Transform cam = Camera.main.transform;

                    TargetGameObject.transform.SetParent(cam);
                    TargetGameObject.GetComponent<WorldBaseCode>().StartChangeEveryThingSmooth(0.5f, 1f, cam.position + cam.forward * 5f + cam.up * -3f, Quaternion.Euler(0f,0f,0f));
                }
                //임시 코드

                CheckRaycast(false);
                SaveAndLoadColor();

                if (EventSystem.current.IsPointerOverGameObject()) return;
                TargetMove(TargetGameObject);
                TargetScale(TargetGameObject);
                TargetTurn(TargetGameObject);

                if (!isSomethingHit)
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                else if (OnRaycast.transform.CompareTag("Block"))
                {
                    if (ReadyToBuildBlock == null)
                    {
                        ReadyToBuildBlock = new GameObject();
                        ReadyToBuildBlock.transform.SetParent(TargetGameObject.transform);

                        ReadyToBuildBlock.transform.localScale = Vector3.one;
                        ReadyToBuildBlock.transform.rotation = new Quaternion(0, 0, 0, 0);

                        MeshFilter mf = ReadyToBuildBlock.AddComponent<MeshFilter>();
                        mf.sharedMesh = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshFilter>().sharedMesh;

                        MeshRenderer mr = ReadyToBuildBlock.AddComponent<MeshRenderer>();
                        mr.sharedMaterials = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshRenderer>().sharedMaterials;

                        ReadyToBuildBlock.AddComponent<SampleBlcok>();
                    }
                    if(showpreviewblock) ReadyToBuildBlock.SetActive(true);

                    Vector3 offset = Vector3.Scale(OnRaycast.normal, TargetGameObject.transform.localScale);
                    ReadyToBuildBlock.transform.position = OnRaycast.transform.position + offset;

                    if(Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
                    {
                        if (DestroyCoroutine != null) StopCoroutine(DestroyCoroutine);
                        DestroyCoroutine = StartCoroutine(DestroyThisBlock());
                    }
                    if ((Input.GetMouseButton(0) && isRotated) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(1))
                    {
                        if (DestroyCoroutine != null)
                        {
                            showpreviewblock = true;

                            StopCoroutine(DestroyCoroutine);
                            DestroyCoroutine = null;

                            if(destroyBlockInDestroy != null) destroyBlockInDestroy.gameObject.transform.localRotation = startRotationInDestroy;
                        }
                    }

                    if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
                    {
                        SpawnNewBlock();

                        Color addcolor = GameManager.Instance.BuildManager.MainColor.color;
                        addcolor.a = GameManager.Instance.BuildManager.alfar;

                        GameManager.Instance.BuildManager.AddPreColor(addcolor);
                    }

                    if (Input.GetMouseButtonDown(2))
                    {
                        GameManager.Instance.BuildManager.SetColor(OnRaycast.transform.GetComponent<MeshRenderer>().material.color);
                    }
                }
                else
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                break;

            case GameFlow.PutWorldInSpace:
                TargetScale(ReadyToBuildBlock);
                TargetTurn(ReadyToBuildBlock);
                TargetMove(ReadyToBuildBlock, false);

                if (Input.GetMouseButtonDown(0)) GameManager.Instance.MainCamera.stopCamera = true;
                if (Input.GetMouseButtonUp(0)) GameManager.Instance.MainCamera.stopCamera = false;
                if (Input.GetMouseButtonUp(1))
                {
                    GameManager.Instance.SaveAndLoadManager.SaveData();
                    GameManager.Instance.ChangeFlow(GameFlow.None);

                    TargetGameObject.transform.parent = GameManager.Instance.WorldManager.SpawnedWorlds.transform;
                    ReadyToBuildBlock.transform.parent = GameManager.Instance.WorldManager.SpawnedWorlds.transform;

                    TargetGameObject.GetComponent<WorldBaseCode>().StartChangeEveryThingSmooth(0.5f, ReadyToBuildBlock.transform.lossyScale.x, ReadyToBuildBlock.transform.position, ReadyToBuildBlock.transform.rotation, true);
                    TargetGameObject.GetComponent<WorldBaseCode>().PutThisWorld();  

                    ChangeToWatchAllWorld();
                }
                break;

            case GameFlow.MakeOneWorld:
                CheckRaycast(false);
                TargetMove(TargetGameObject);
                TargetScale(TargetGameObject);
                TargetTurn(TargetGameObject);

                if (!isSomethingHit)
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                else if (OnRaycast.transform.CompareTag("Block"))
                {
                    if (ReadyToBuildBlock == null)
                    {
                        ReadyToBuildBlock = new GameObject();
                        ReadyToBuildBlock.transform.SetParent(TargetGameObject.transform);

                        ReadyToBuildBlock.transform.localScale = Vector3.one;
                        ReadyToBuildBlock.transform.rotation = new Quaternion(0,0,0,0);

                        MeshFilter mf = ReadyToBuildBlock.AddComponent<MeshFilter>();
                        mf.sharedMesh = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshFilter>().sharedMesh;

                        MeshRenderer mr = ReadyToBuildBlock.AddComponent<MeshRenderer>();
                        mr.sharedMaterials = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshRenderer>().sharedMaterials;

                        ReadyToBuildBlock.AddComponent<SampleBlcok>();
                    }

                    ReadyToBuildBlock.SetActive(true);
                    Vector3 offset = Vector3.Scale(OnRaycast.normal, TargetGameObject.transform.localScale);
                    ReadyToBuildBlock.transform.position = OnRaycast.transform.position + offset;
                }
                else
                {
                    if (ReadyToBuildBlock == null) return;
                    ReadyToBuildBlock.SetActive(false);
                }
                break;
        }
    }

    void ChangeToWatchAllWorld()
    {
        Destroy(ReadyToBuildBlock);
        ReadyToBuildBlock = null;
        TargetGameObject = null;

        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }

    public void SettingWhenChangeToPutWorld()
    {
        ReadyToBuildBlock = TargetGameObject.GetComponent<WorldBaseCode>().MakePreview(7);

        ReadyToBuildBlock.transform.position = Camera.main.transform.forward * 5f;
    }

    void SaveAndLoadColor()
    {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            SaveColors();
        }
        else
        {
            LoadColor();
        }
    }

    void SaveColors()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) GameManager.Instance.BuildManager.AddSaveColor(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) GameManager.Instance.BuildManager.AddSaveColor(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) GameManager.Instance.BuildManager.AddSaveColor(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) GameManager.Instance.BuildManager.AddSaveColor(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) GameManager.Instance.BuildManager.AddSaveColor(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) GameManager.Instance.BuildManager.AddSaveColor(5);
    }

    void LoadColor()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2)) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[1]);
        if (Input.GetKeyDown(KeyCode.Alpha3)) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[2]);
        if (Input.GetKeyDown(KeyCode.Alpha4)) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[3]);
        if (Input.GetKeyDown(KeyCode.Alpha5)) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[4]);
        if (Input.GetKeyDown(KeyCode.Alpha6)) GameManager.Instance.BuildManager.SetColor(GameManager.Instance.BuildManager.SaveColors[5]);
    }

    public void TargetMove(GameObject _target, bool isMoveSide = true)
    {
        Vector3 move = Vector3.zero;
        if (isMoveSide)
        {
            if (Input.GetKey(MoveUp)) move += Camera.main.transform.up;
            if (Input.GetKey(MoveDown)) move -= Camera.main.transform.up;
            if (Input.GetKey(MoveRight)) move += Camera.main.transform.right;
            if (Input.GetKey(MoveLeft)) move -= Camera.main.transform.right;
        }
        if (Input.GetKey(MoveFar)) move += Camera.main.transform.forward;
        if (Input.GetKey(MoveClose)) move -= Camera.main.transform.forward;
        if (move != Vector3.zero)
        {
            _target.transform.position += move.normalized * TargetMoveSensitive * Time.deltaTime;
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

        if (Input.GetMouseButton(0))
        {
            isRotating = true;
            isRotated = false;
        }

        if (isRotating && _target != null)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            Transform cam = Camera.main.transform;

            if(mouseX >= 0.2f || mouseX <= -0.2f || mouseY >= 0.2f || mouseY <= -0.2f)
            {
                isRotated = true;
            }

            _target.transform.Rotate(
                cam.up,
                -mouseX * rotateSpeed,
                Space.World
            );

            _target.transform.Rotate(
                cam.right,
                mouseY * rotateSpeed,
                Space.World
            );
        }
    }

    void CheckRaycast(bool isCamera)
    {
        Ray ray = new Ray();
        if (isCamera) ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        else ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit; 
        
        if (Physics.Raycast(ray, out hit))
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
        BoxCollider box = TargetGameObject.GetComponent<BoxCollider>();

        float baseDistance = 2f; // 최소 거리
        float radius = box.bounds.extents.magnitude;

        Vector3 targetPos = cam.position + cam.forward * (baseDistance + radius);

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

        TargetGameObject.GetComponent<WorldBaseCode>().ChooseThisWorld();
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

    Quaternion startRotationInDestroy;
    GameObject destroyBlockInDestroy;

    IEnumerator DestroyThisBlock()
    {
        destroyBlockInDestroy = OnRaycast.collider.gameObject;
        Vector3 normal = OnRaycast.normal.normalized;

        startRotationInDestroy = destroyBlockInDestroy.transform.localRotation;

        float duration = 0.2f;
        float frequency = 8f;   // 흔들리는 속도
        float angle = 5f;      // 고개 꺾이는 각도

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

            destroyBlockInDestroy.transform.localRotation = startRotationInDestroy * Quaternion.AngleAxis(swing, normal);

            yield return null;
        }

        TargetGameObject.GetComponent<WorldBaseCode>().DestroyBlock(destroyBlockInDestroy);
        destroyBlockInDestroy = null;
        showpreviewblock = true;
    }

    void SpawnNewBlock()
    {
        Color SpawnedColor = GameManager.Instance.BuildManager.MainColor.color;
        float alfar = GameManager.Instance.BuildManager.alfar;

        TargetGameObject.GetComponent<WorldBaseCode>().MakeNewBlocks(GameManager.Instance.BuildManager.ChooseBlock,
            ReadyToBuildBlock.transform.localPosition,
            SpawnedColor,
            alfar
            );
    }
}
