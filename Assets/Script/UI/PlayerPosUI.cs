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

    WorldExUI LastLook;
    public GameObject SelectBtn;

    [Header("Sound")]
    public SoundData OnLookWorldSound;
    public SoundData ResetPosSound;

    public override void OnSetting()
    {
        Clearworldexui();
        LastLook = null;

        for (int i = 0; i< GameManager.Instance.WorldManager.Worlds.Count; i++)
        {
            GameObject ui = Instantiate(WorldExUI, WorldsCheckUI.transform);
            ui.GetComponent<WorldExUI>().Setting(GameManager.Instance.WorldManager.Worlds[i], this);
            WorldsCheckUI.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 175f);

            worldexuis.Add(ui);
        }

        SelectBtn.SetActive(false);
    }

    void Clearworldexui()
    {
        if (worldexuis == null) worldexuis = new List<GameObject>();

        if(worldexuis.Count == 0) return;
        foreach(GameObject go in worldexuis)
        {
            WorldsCheckUI.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 175f);
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
        blockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, GameManager.Instance.UIManager.SelectionUI.transform);

        Transform Cam = GameManager.Instance.MainCamera.transform;

        Vector3 StartPos = Cam.position;
        Vector3 TargetPos = GameManager.Instance.MainCamera.PlayerBeforePos;

        Quaternion StartRot = Cam.rotation;
        Quaternion TargetRot = GameManager.Instance.MainCamera.PlayerBeforeRot;

        float duration = 0.5f;
        float time = 0f;

        GameManager.Instance.SoundManager.PlayEffect(ResetPosSound, 0.3f, 2.2f);

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

    public void TryToLookAt(WorldExUI _give)
    {
        if (_give == null)
            return;

        if (!_give.CanSelect())
        {
            SelectBtn.SetActive(false);
            return;
        }

        if (LastLook != _give)
        {
            LastLook = _give;

            foreach (GameObject weu in worldexuis)
            {
                weu.GetComponent<WorldExUI>()
                    .OnSelect.SetActive(false);
            }

            _give.LooAtThis();

            GameManager.Instance.SoundManager
                .PlayEffect(OnLookWorldSound);

            SelectBtn.SetActive(true);
        }
    }

    public void ClickSelectBtn()
    {
        if (LastLook == null)
            return;

        if (!LastLook.CanSelect())
        {
            SelectBtn.SetActive(false);
            return;
        }

        GameManager.Instance.InputManager.TargetGameObject =
            LastLook.getChunkCode().gameObject;

        GameManager.Instance.ChangeFlow(
            GameFlow.SelectOneWorld
        );
    }
}
