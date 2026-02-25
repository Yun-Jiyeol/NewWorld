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
    public float PlayerMoveSpeed = 1f;

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

    private void Update()
    {
        if(isTouchingUI) return;

        switch (GameManager.Instance.flow)
        {
            case GameFlow.WatchAllWorld:
                if (Cursor.visible)
                {
                    if (Input.GetKeyDown(KeyCode.F1))//나중 esc로 변경
                    {
                        GameManager.Instance.UIManager.SelectionUI.PopInSettingPanel();
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                        GameManager.Instance.MainCamera.stopCamera = false;
                    }
                }
                else
                {
                    CheckRaycast(true);
                    PlayerMove();

                    if (Input.GetKeyDown(BuildNewWorld))
                    {
                        GameObject newworld = GameManager.Instance.WorldManager.MakeNewWorld(3, 3, 3);

                        TargetGameObject = newworld;
                        TargetGameObject.GetComponent<ChunkCode>().ChooseThisWorld();
                        GameManager.Instance.ChangeFlow(GameFlow.MakeOneWorld);
                    }
                    if (Input.GetKeyDown(KeyCode.F1)) //나중 esc로 변경
                    {
                        GameManager.Instance.UIManager.SelectionUI.PopUpSettingPanel();
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.Confined;
                        GameManager.Instance.MainCamera.stopCamera = true;
                    }

                    if (!isSomethingHit) return;
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (OnRaycast.transform.CompareTag("World"))
                        {
                            TargetGameObject = OnRaycast.transform.gameObject;
                            Camera.main.transform.LookAt(TargetGameObject.transform.position);
                            GameManager.Instance.ChangeFlow(GameFlow.SelectOneWorld);
                        }
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
                    if(ReadyToDestroyBlock != null)
                    {
                        Destroy(ReadyToDestroyBlock);
                        ReadyToDestroyBlock = null;
                    }

                    GameManager.Instance.ChangeFlow(GameFlow.None);

                    GameObject cam = GameManager.Instance.MainCamera.gameObject;

                    TargetGameObject.transform.SetParent(cam.transform);
                    TargetGameObject.GetComponent<ChunkCode>().StartChangeEveryThingSmooth(0.5f,true, 1f, true ,cam.transform.position + cam.transform.forward * 25f + cam.transform.up * 1f, true, Quaternion.Euler(0f,0f,0f));
                    Invoke("ChangeToPutWorldInSpace", 0.7f);
                }
                //임시 코드

                CheckRaycast(false);
                SaveAndLoadColor();

                if (EventSystem.current.IsPointerOverGameObject()) return;
                TargetMove(TargetGameObject);
                TargetScale(TargetGameObject);
                TargetTurn(TargetGameObject);
                if (Input.GetKey(KeyCode.Delete))
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

                    if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
                        {
                            if(BeforeDestroyBlockPos == null)
                            {
                                SettingDestroyRange();
                            }
                            else
                            {
                                SettingDestroyRangeAfter();
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
                        {
                            SettingDestroyRange();
                        }
                    }

                    /*
                    if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
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

                            if(block != null)
                            {
                                GameManager.Instance.BuildManager.RequestBuild
                                    (new WorldAndBlock
                                    {
                                        world = TargetGameObject.GetComponent<ChunkCode>(),
                                        blockposition = block._position,
                                        blockcolor = block._color
                                    });

                                Destroy(block._go);
                                block = null;
                            }
                        }
                    }*/

                    if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
                    {
                        ResetDestroyRange();
                        SpawnNewBlock();

                        Color addcolor = GameManager.Instance.BuildManager.MainColor.color;
                        addcolor.a = GameManager.Instance.BuildManager.alfar;

                        GameManager.Instance.BuildManager.AddPreColor(addcolor);
                    }

                    if (Input.GetMouseButtonDown(2))
                    {
                        ResetDestroyRange();
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
                TargetScale(TargetGameObject);
                TargetTurn(TargetGameObject);
                TargetMove(TargetGameObject, false);

                if (Input.GetMouseButtonDown(0)) GameManager.Instance.MainCamera.stopCamera = true;
                if (Input.GetMouseButtonUp(0)) GameManager.Instance.MainCamera.stopCamera = false;
                if (Input.GetMouseButtonUp(1))
                {
                    GameManager.Instance.SaveAndLoadManager.SaveData();
                    GameManager.Instance.ChangeFlow(GameFlow.None);

                    TargetGameObject.transform.parent = GameManager.Instance.WorldManager.SpawnedWorlds.transform;

                    TargetGameObject.GetComponent<ChunkCode>().PutThisWorld();  

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
        if(AfterDestroyBlockPos != null)
        {
            BeforeDestroyBlockPos = AfterDestroyBlockPos;
        }

        AfterDestroyBlockPos = ReadyToBuildBlock.transform.localPosition;
        if(AfterDestroyBlockPos == BeforeDestroyBlockPos)
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
        Destroy(ReadyToBuildBlock);
        ReadyToBuildBlock = null;
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

    public void PlayerMove()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(MoveUp)) move += Camera.main.transform.forward;
        if (Input.GetKey(MoveDown)) move -= Camera.main.transform.forward;
        if (Input.GetKey(MoveRight)) move += Camera.main.transform.right;
        if (Input.GetKey(MoveLeft)) move -= Camera.main.transform.right;

        Camera.main.transform.position += move.normalized * Time.deltaTime * PlayerMoveSpeed;
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
}
