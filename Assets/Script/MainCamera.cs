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

    float yaw;   // ÁÂ¿ì
    float pitch; // »óÇÏ

    public bool stopCamera = true;

    public Vector3 PlayerBeforePos;
    public Quaternion PlayerBeforeRot;

    void Update()
    {
        if(GameManager.Instance.flow == GameFlow.WatchOneWorld
            || GameManager.Instance.flow == GameFlow.None
            || GameManager.Instance.flow == GameFlow.Start
            || GameManager.Instance.flow == GameFlow.MakeOneWorld
            || GameManager.Instance.flow == GameFlow.SelectOneWorld) return;
        if (stopCamera) return;
        if (isRotating) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * 100f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void LookAt(Quaternion quaternion)
    {
        Vector3 angles = quaternion.eulerAngles;

        yaw = angles.y;

        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
    }

    public void TryCheckrot()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;

        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
    }

    public void SavePos()
    {
        PlayerBeforePos = gameObject.transform.position;
        PlayerBeforeRot = gameObject.transform.rotation;
    }

    public void LookAt(Vector3 pos, float dur = 0.5f)
    {
        StartCoroutine(RotateTo(pos, dur));
    }

    bool isRotating = false;
    IEnumerator RotateTo(Vector3 targetPosition, float duration)
    {
        isRotating = true;

        Transform cam = transform;

        Quaternion startRot = cam.rotation;

        Vector3 dir = (targetPosition - cam.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            cam.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        cam.rotation = targetRot;

        TryCheckrot();

        isRotating = false;
    }
}
