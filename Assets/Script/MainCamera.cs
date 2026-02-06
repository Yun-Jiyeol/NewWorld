using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float sensitivity = 3f;
    public float minPitch = -90f;
    public float maxPitch = 90f;

    float yaw;   // ÁÂ¿ì
    float pitch; // »óÇÏ

    public bool stopCamera = false;
    public GameObject SubCamera;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
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
}
