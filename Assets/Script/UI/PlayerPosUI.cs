using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPosUI : BasicUI
{
    public GameObject WorldExUI;
    public GameObject WorldsCheckUI;

    List<GameObject> worldexuis;
    GameObject blockImage;

    public override void OnSetting()
    {
        Clearworldexui();
        for (int i = 0; i< GameManager.Instance.WorldManager.Worlds.Count; i++)
        {
            GameObject ui = Instantiate(WorldExUI, WorldsCheckUI.transform);
            ui.GetComponent<WorldExUI>().Setting(GameManager.Instance.WorldManager.Worlds[i]);

            worldexuis.Add(ui);
        }
    }

    void Clearworldexui()
    {
        if (worldexuis == null) worldexuis = new List<GameObject>();

        if(worldexuis.Count == 0) return;
        foreach(GameObject go in worldexuis)
        {
            Destroy(go);
        }

        worldexuis.Clear();
    }

    public void ResetCameraPos()
    {
        StartCoroutine(ResetCameraPosCR());
    }

    IEnumerator ResetCameraPosCR()
    {
        blockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, gameObject.transform);

        Transform Cam = GameManager.Instance.MainCamera.transform;

        Vector3 StartPos = Cam.position;
        Vector3 TargetPos = Vector3.zero;

        Quaternion StartRot = Cam.rotation;
        Quaternion TargetRot = Quaternion.Euler(0, 0, 0);

        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            Cam.rotation = Quaternion.Slerp(StartRot, TargetRot, t);
            Cam.position = Vector3.Slerp(StartPos, TargetPos, t);

            yield return null;
        }

        Cam.position = TargetPos;
        Cam.rotation = TargetRot;

        GameManager.Instance.MainCamera.TryCheckrot();

        Destroy(blockImage);
        blockImage = null;
    }
}
