using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum SystemName
{
    _Reverse,
    _NoneReverse,
    _BlackWhite,
    _NoneBlackWhite,
    _Contrast,
    _ContrastThreshold,
    _Posterize,
    _PosterizeSteps,
    _Pixelate,
    _PixelSize,
    _Vignette,
    _VignetteStrength,
    _Edge,
    _EdgeStrength,
    _Noise,
    _NoiseStrength,
    _CRT,
    _CRTStrength
}

public class MainCamera : MonoBehaviour
{
    public GameObject SubCamera;

    public float sensitivity = 3f;
    public float minPitch = -90f;
    public float maxPitch = 90f;

    float yaw;   // 좌우
    float pitch; // 상하
    float roll; // 회전

    public bool stopCamera = true;

    public Vector3 PlayerBeforePos;
    public Quaternion PlayerBeforeRot;

    KeyManager _keyManager;

    private void Start()
    {
        _keyManager = GameManager.Instance.KeyManager;
    }

    void Update()
    {
        if(GameManager.Instance.flow == GameFlow.WatchOneWorld
            || GameManager.Instance.flow == GameFlow.None
            || GameManager.Instance.flow == GameFlow.Start
            || GameManager.Instance.flow == GameFlow.MakeOneWorld
            || GameManager.Instance.flow == GameFlow.SelectOneWorld
            || GameManager.Instance.flow == GameFlow.PutWorldInSpace) return;
        if (stopCamera) return;
        if (isRotating) return;
        CameraTurn();
    }

    public void CameraTurn()
    {
        if (_keyManager.isGetKey(_keyManager.KeyboardDictionary[KeyNameSaves.CameraTurn]))
        {
            CameraTurn180();
            return;
        }
        CameraMouseTurn();
    }

    void CameraTurn180()
    {
        stopCamera = true;

        Vector3 rot = transform.eulerAngles;

        rot.z += 180f;

        transform.DORotate(
            rot,
            0.5f,
            RotateMode.FastBeyond360
        )
        .OnComplete(() =>
        {
            TryCheckrot();
            stopCamera = false;
        });
    }

    void CameraMouseTurn()
    {
        float mouseX =
            Input.GetAxis("Mouse X")
            * sensitivity
            * 100f
            * Time.deltaTime;

        float mouseY =
            Input.GetAxis("Mouse Y")
            * sensitivity
            * 100f
            * Time.deltaTime;

        // 카메라 뒤집힘 체크
        float upDot =
            Vector3.Dot(transform.up, Vector3.up);

        // 뒤집혔으면 yaw 반전
        if (upDot < 0f)
        {
            mouseX = -mouseX;
        }

        if(roll > 90f || roll < -90f)
        {
            mouseY = -mouseY;
        }

        yaw += mouseX;
        pitch -= mouseY;

        transform.rotation = Quaternion.Euler(pitch, yaw, roll);
    }

    public void LookAt(Quaternion quaternion)
    {
        Vector3 angles = quaternion.eulerAngles;

        yaw = angles.y;

        pitch = angles.x > 180f ? angles.x - 360f : angles.x;

        roll = angles.z > 180f ? angles.z - 360f : angles.z;
    }

    public void TryCheckrot()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;

        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
        
        roll = angles.z > 180f? angles.z - 360f : angles.z;
    }

    public void SavePos()
    {
        PlayerBeforePos = gameObject.transform.position;
        PlayerBeforeRot = gameObject.transform.rotation;
    }

    public void LookAt(Transform targetobject, float dur = 0.5f)
    {
        StartCoroutine(RotateTo(targetobject, dur));
    }

    bool isRotating = false;
    IEnumerator RotateTo(Transform targetobject, float duration)
    {
        isRotating = true;

        Vector3 targetPos = GetCameraPosition(targetobject, gameObject.GetComponent<Camera>());

        Transform cam = transform;

        Quaternion startRot = cam.rotation;
        Vector3 startPos = cam.position;

        Vector3 dir = (targetobject.position - targetPos).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            cam.position = Vector3.Lerp(startPos, targetPos, t);
            cam.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        cam.position = targetPos;
        cam.rotation = targetRot;

        TryCheckrot();

        isRotating = false;
    }

    public Vector3 GetCameraPosition(Transform target, Camera cam, float padding = 1.2f)
    {
        // Renderer bounds 가져오기
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return target.position;

        Bounds bounds = renderers[0].bounds;

        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Vector3 center = bounds.center;
        float radius = bounds.extents.magnitude;

        // 카메라 FOV 기준 거리 계산
        float fov = cam.fieldOfView * Mathf.Deg2Rad;
        float distance = radius / Mathf.Sin(fov / 2f);

        distance *= padding; // 여유 거리

        // 카메라 방향 (현재 방향 유지)
        Vector3 dir = cam.transform.forward;

        // 최종 위치 (뒤로 물러남)
        Vector3 pos = center - dir * distance;

        return pos;
    }
}
