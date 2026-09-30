using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DG.Tweening.DOTweenAnimation;
using static UnityEngine.GraphicsBuffer;

public class LightUI : BasicUI
{
    public CopyChoose CopyChoose;
    public CopyLine CopyLine;
    public ClickImagetoChange RightClick;
    public ClickImagetoChange LeftClick;

    public ClickImageToChangeRange RightRangeClick;
    public ClickImageToChangeRange LeftRangeClick;

    public ClickingImageToCoroutine RightAngleClick;
    public ClickingImageToCoroutine LeftAngleClick;

    public TextMeshProUGUI showSettingLight;
    public TMP_InputField showSettingLightInput;

    public TextMeshProUGUI showSettingRange;
    public TMP_InputField showSettingRangeInput;

    public GameObject SettingAngleExplainText;
    public GameObject SettingAngleGo;
    public TextMeshProUGUI showSettingAngle;
    public TMP_InputField showSettingAngleInput;

    public ChangeLightTypeButton Spot;
    public ChangeLightTypeButton Point;

    public RectTransform OnChangeRect;

    float SettingLight;
    float SettingRange;
    float SettingAngle;
    int MaxLight = 9999;
    int MinLight = 0;

    public Light target;
    LightType targettype;

    public Color _color;
    public Color inLineChoose;

    public Image BGColor;

    GameObject ShowLight;
    bool isLightMove = true;
    bool isFirstLightSetting = false;

    Vector3 resetLightPosition;
    bool hasResetLightPosition = false;

    KeyManager _keyManager;

    [Header("Sound")]
    public SoundData ChangeTypeSound;
    public SoundData ChangeValueSound;
    public SoundData SettingValueSound;
    public SoundData ResetSound;

    public override void OnSetting()
    {
        if (GameManager.Instance.InputManager.TargetGameObject == null)
            return;

        ChunkCode chunk =
            GameManager.Instance.InputManager
                .TargetGameObject
                .GetComponent<ChunkCode>();


        // ChangeLightPower()가 Light를 만들기 전에 판단
        bool isNewLight =
            chunk.LightBlock == null;


        target =
            chunk.ChangeLightPower();


        _keyManager =
            GameManager.Instance.KeyManager;

        SettingLight =
            (int)target.intensity;

        _color =
            target.color;

        targettype =
            target.type;

        SettingRange =
            target.range;

        SettingAngle =
            target.spotAngle;

        isLightMove = true;


        // Reset 기준 초기화
        hasResetLightPosition = false;


        // 원래부터 존재하던 Light라면
        // UI에 들어온 바로 이 위치가 Reset 기준
        if (!isNewLight)
        {
            resetLightPosition =
                target.transform.position;

            hasResetLightPosition =
                true;
        }


        CopyLine.Setting(this);
        CopyChoose.Setting(this);

        RightClick.Setting(this);
        LeftClick.Setting(this);

        RightRangeClick.Setting(this);
        LeftRangeClick.Setting(this);

        RightAngleClick.Setting(this);
        LeftAngleClick.Setting(this);

        Spot.Setting(this);
        Point.Setting(this);

        SettingType();

        ShowSettingLightRange();
        ShowSettingLight();
        ShowSettingLightAngle();


        // 새 Light라면
        // 자동 배치가 끝난 뒤 Reset 위치를 정함
        if (isNewLight)
        {
            StartCoroutine(
                SettingFirstLight(chunk)
            );
        }
    }

    IEnumerator SettingFirstLight(
    ChunkCode chunk
)
    {
        isFirstLightSetting = true;


        if (!chunk.TryGetWorldBounds(
            out Bounds bounds
        ))
        {
            isFirstLightSetting = false;
            yield break;
        }


        Camera cam =
            Camera.main;

        Transform camTransform =
            cam.transform;


        // -------------------------
        // 1. 월드를 바라볼 방향 결정
        // -------------------------

        Vector3 viewDirection;

        // 플레이어가 물체 안에 있으면
        // 현재 카메라 방향을 기준으로 밖으로 빼냄
        if (bounds.Contains(camTransform.position))
        {
            viewDirection =
                camTransform.forward;
        }
        else
        {
            viewDirection =
                bounds.center -
                camTransform.position;
        }


        if (viewDirection.sqrMagnitude < 0.001f)
        {
            viewDirection =
                camTransform.forward;
        }

        viewDirection.Normalize();


        // -------------------------
        // 2. 월드 전체가 보이는 거리 계산
        // -------------------------

        float radius =
            bounds.extents.magnitude;


        float verticalFov =
            cam.fieldOfView *
            Mathf.Deg2Rad;


        float horizontalFov =
            2f *
            Mathf.Atan(
                Mathf.Tan(
                    verticalFov * 0.5f
                )
                * cam.aspect
            );


        float useFov =
            Mathf.Min(
                verticalFov,
                horizontalFov
            );


        float distance =
            radius /
            Mathf.Sin(
                useFov * 0.5f
            );


        // 화면 가장자리에 너무 딱 붙지 않게
        distance *= 1.2f;


        Vector3 targetCameraPosition =
            bounds.center -
            viewDirection * distance;


        Quaternion targetCameraRotation =
            Quaternion.LookRotation(
                bounds.center -
                targetCameraPosition,
                Vector3.up
            );


        // -------------------------
        // 3. 카메라 부드럽게 이동
        // -------------------------

        Vector3 startPosition =
            camTransform.position;

        Quaternion startRotation =
            camTransform.rotation;


        float duration = 0.5f;
        float time = 0f;


        while (time < duration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time / duration
                );


            camTransform.position =
                Vector3.Lerp(
                    startPosition,
                    targetCameraPosition,
                    t
                );


            camTransform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetCameraRotation,
                    t
                );


            yield return null;
        }


        camTransform.position =
            targetCameraPosition;

        camTransform.rotation =
            targetCameraRotation;


        // -------------------------
        // 4. 새 시야 기준으로 Light 위치 결정
        // -------------------------

        SetFirstLightPosition(
       chunk,
       bounds,
       cam
   );


        // 최초 자동 생성된 위치를
        // Reset 기준 위치로 저장
        resetLightPosition =
            target.transform.position;

        hasResetLightPosition =
            true;


        GameManager.Instance.MainCamera
            .TryCheckrot();


        isFirstLightSetting = false;


        // 카메라 내부 회전값 보정
        GameManager.Instance.MainCamera
            .TryCheckrot();


        isFirstLightSetting = false;
    }

    void SetFirstLightPosition(
    ChunkCode chunk,
    Bounds bounds,
    Camera cam
)
    {
        if (chunk.LightBlock == null)
            return;


        // 화면 기준 위쪽
        // +
        // 약간 카메라 쪽
        Vector3 direction =
            (
                cam.transform.up
                -
                cam.transform.forward * 0.35f
            ).normalized;


        Vector3 extents =
            bounds.extents;


        float tx =
            Mathf.Abs(direction.x) > 0.001f
            ? extents.x /
              Mathf.Abs(direction.x)
            : float.PositiveInfinity;


        float ty =
            Mathf.Abs(direction.y) > 0.001f
            ? extents.y /
              Mathf.Abs(direction.y)
            : float.PositiveInfinity;


        float tz =
            Mathf.Abs(direction.z) > 0.001f
            ? extents.z /
              Mathf.Abs(direction.z)
            : float.PositiveInfinity;


        // Bounds 중심에서 해당 방향으로
        // 표면까지의 거리
        float surfaceDistance =
            Mathf.Min(
                tx,
                Mathf.Min(ty, tz)
            );


        Vector3 surfacePosition =
            bounds.center +
            direction * surfaceDistance;


        float worldSize =
            Mathf.Max(
                bounds.size.x,
                bounds.size.y,
                bounds.size.z
            );


        // 표면에서 조금 더 바깥
        float margin =
            Mathf.Max(
                1f,
                worldSize * 0.15f
            );


        chunk.LightBlock.transform.position =
            surfacePosition +
            direction * margin;
    }

    private void Update()
    {
        if (isFirstLightSetting)
            return;

        if (target == null) return;
        if (EventSystem.current.IsPointerOverGameObject())
        {
            GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(CoinType.None);
            return;
        }

        if (_keyManager.isGetKeyDown(_keyManager.KeyboardDictionary[KeyNameSaves.PopUpUI]))
        {
            isLightMove = !isLightMove;
        }

        if (isLightMove)
        {
            GameManager.Instance.InputManager.nowMovingThing = CoinType.Light;
            GameManager.Instance.InputManager.TargetMove(target.gameObject);
            GameManager.Instance.InputManager.TargetTurn(target.gameObject);
        }
        else
        {
            GameManager.Instance.InputManager.nowMovingThing = CoinType.Player;
            GameManager.Instance.InputManager.PlayerMove();
            GameManager.Instance.MainCamera.CameraTurn();
        }
    }

    void MakeSpere()
    {
        if(ShowLight != null) Destroy(ShowLight);

        if (targettype == LightType.Spot)
        {
            ShowLight = Instantiate(GameManager.Instance.LightManager.RayLight, target.transform);
        }
        if (targettype == LightType.Point)
        {
            ShowLight = Instantiate(GameManager.Instance.LightManager.SphereLight, target.transform);
        }

        SetLayerAll(ShowLight, 6);

        Vector3Int SZ = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>().GiveThreeSize();
        float maxSZ = Mathf.Max(SZ.x, SZ.y, SZ.z);

        ShowLight.transform.localScale = Vector3.one  * (maxSZ / 10f);
        SettingColor();
    }

    public void ChangeLineChoose(Color color)
    {
        inLineChoose = color;
        BGColor.color = color;
    }

    public void ChangeSettingLight(float i)
    {
        SettingLight += i;

        if (SettingLight < MinLight)
        {
            SettingLight = MinLight;
        }
        if (SettingLight > MaxLight)
        {
            SettingLight = MaxLight;
        }

        target.intensity = SettingLight;
        GameManager.Instance.SoundManager.PlayEffect(ChangeValueSound, 0.1f);

        ShowSettingLight();
    }

    public void ChangeSettingLightNum()
    {
        if (!float.TryParse(showSettingLightInput.text, out float x))
        {
            showSettingLightInput.text = "";
            return;
        }

        SettingLight = x;

        if (SettingLight < MinLight)
        {
            SettingLight = MinLight;
        }
        if (SettingLight > MaxLight)
        {
            SettingLight = MaxLight;
        }

        target.intensity = SettingLight;
        GameManager.Instance.SoundManager.PlayEffect(SettingValueSound);

        ShowSettingLight();
    }

    void ShowSettingLight()
    {
        if (SettingLight == 0)
        {
            target.enabled = false;
        }
        else
        {
            target.enabled = true;
        }

        showSettingLightInput.text = "";
        showSettingLight.text = SettingLight.ToString("F1");
    }

    public void ChangeSettingLightRange(float i)
    {
        SettingRange += i;

        if (SettingRange < MinLight)
        {
            SettingRange = MinLight;
        }
        if (SettingRange > MaxLight)
        {
            SettingRange = MaxLight;
        }

        target.range = SettingRange * 10f;
        GameManager.Instance.SoundManager.PlayEffect(ChangeValueSound, 0.1f);

        ShowSettingLightRange();
    }

    public void ChangeSettingLightRangeNum()
    {
        if (!float.TryParse(showSettingRangeInput.text, out float x))
        {
            showSettingRangeInput.text = "";
            return;
        }

        SettingRange = x;

        if (SettingRange < MinLight)
        {
            SettingRange = MinLight;
        }
        if (SettingRange > MaxLight)
        {
            SettingRange = MaxLight;
        }

        target.range = SettingRange * 10f;
        GameManager.Instance.SoundManager.PlayEffect(SettingValueSound);

        ShowSettingLightRange();
    }

    void ShowSettingLightRange()
    {
        if (SettingRange == 0)
        {
            target.enabled = false;
        }
        else
        {
            target.enabled = true;
        }

        showSettingRangeInput.text = "";
        showSettingRange.text = SettingRange.ToString("F1");
    }

    public void ChangeSettingLightAngle(float i)
    {
        SettingAngle += i;

        if (SettingAngle < 0)
        {
            SettingAngle = 0;
        }
        if (SettingAngle > 179)
        {
            SettingAngle = 179;
        }

        target.spotAngle = SettingAngle;
        GameManager.Instance.SoundManager.PlayEffect(ChangeValueSound, 0.1f);

        ShowSettingLightAngle();
    }

    public void ChangeSettingLightAngleNum()
    {
        if (!float.TryParse(showSettingAngleInput.text, out float x))
        {
            showSettingAngleInput.text = "";
            return;
        }

        SettingAngle = x;

        if (SettingAngle < 0)
        {
            SettingAngle = 0;
        }
        if (SettingAngle > 179)
        {
            SettingAngle = 179;
        }

        target.spotAngle = SettingAngle;
        GameManager.Instance.SoundManager.PlayEffect(SettingValueSound);

        ShowSettingLightAngle();
    }

    void ShowSettingLightAngle()
    {
        if (SettingAngle == 0)
        {
            target.enabled = false;
        }
        else
        {
            target.enabled = true;
        }

        showSettingAngleInput.text = "";
        showSettingAngle.text = SettingAngle.ToString("F1");
    }

    void SetLayerAll(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerAll(child.gameObject, layer);
        }
    }

    public void ChangeColor()
    {
        target.color = _color;
        SettingColor();
    }

    void SettingColor()
    {
        if (ShowLight == null) return;

        Color showcolor = _color;
        showcolor.a = 1f;
        GameManager.Instance.MaterialManager.LightMaterial.color = showcolor;
    }

    public void ChangeType(LightType _type)
    {
        if(targettype == _type) return;
        targettype = _type;
        target.type = _type;
        GameManager.Instance.SoundManager.PlayEffect(ChangeTypeSound);
        SettingType();
    }

    void SettingType()
    {
        switch (targettype)
        {
            case (LightType.Spot):
                OnChangeRect.sizeDelta = new Vector2(OnChangeRect.sizeDelta.x, 930);
                Spot.gameObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(185f, 47f), 0.3f);
                Point.gameObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(47f, 47f), 0.3f);
                SettingAngleExplainText.SetActive(true);
                SettingAngleGo.SetActive(true);
                MakeSpere();
                SettingColor();
                break;

            case (LightType.Point):
                OnChangeRect.sizeDelta = new Vector2(OnChangeRect.sizeDelta.x, 825);
                Spot.gameObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(47f, 47f), 0.3f);
                Point.gameObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(185f, 47f), 0.3f);
                SettingAngleExplainText.SetActive(false);
                SettingAngleGo.SetActive(false);
                MakeSpere();
                SettingColor();
                break;
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.UIManager.WhatMoving.ChangeMoveType(CoinType.None);
        if (ShowLight != null) Destroy(ShowLight);
    }

    public void ResetPosition()
    {
        if (target == null)
            return;

        // 최초 Light 위치 계산이 아직 안 끝났으면
        // Reset 하지 않음
        if (isFirstLightSetting)
            return;

        if (!hasResetLightPosition)
            return;


        GameManager.Instance.SoundManager
            .PlayEffect(ResetSound);


        target.transform.position =
            resetLightPosition;
    }

    public void OnSelection(GameObject _go)
    {
        _go.SetActive(true);
    }

    public void DisSelection(GameObject _go)
    {
        _go.SetActive(false);
    }
}
