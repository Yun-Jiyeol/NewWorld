using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public GameObject SubCamera;

    public float sensitivity = 3f;
    public float minPitch = -90f;
    public float maxPitch = 90f;

    float yaw;   // 좌우
    float pitch; // 상하

    public bool stopCamera = false;

    void Start()
    {
        TryCheckrot();
    }

    void Update()
    {
        if(GameManager.Instance.flow == GameFlow.WatchOneWorld
            || GameManager.Instance.flow == GameFlow.None
            || GameManager.Instance.flow == GameFlow.Start
            || GameManager.Instance.flow == GameFlow.MakeOneWorld
            || GameManager.Instance.flow == GameFlow.SelectOneWorld) return;
        if (stopCamera) return;


        float mouseX = Input.GetAxis("Mouse X") * sensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * 100f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void TryCheckrot()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    public void LookAt(Vector3 pos)
    {
        StartCoroutine(RotateTo(pos, 0.5f));
    }

    IEnumerator RotateTo(Vector3 targetPosition, float duration)
    {
        Transform cam = Camera.main.transform;

        Quaternion startRot = cam.rotation;

        Vector3 dir = (targetPosition - cam.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 부드러운 보간
            cam.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        cam.rotation = targetRot; // 마지막 보정
        TryCheckrot();
    }
}
