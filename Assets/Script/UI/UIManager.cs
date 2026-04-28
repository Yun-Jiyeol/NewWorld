using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneSelection
{
    None,
    StartScene,
    InGameUI
}

public class UIManager : MonoBehaviour
{
    public GameObject PreviewWorld;
    public GameObject Compass;
    public SelectionUI SelectionUI;

    public GameObject BlockImage;
    public GameObject SavedImage;

    public WorldSeenUI WorldSeenUI;

    public SavingEffectUI SavingEffectUI;
    public WhatMoving WhatMoving;

    public GameObject StartScene;
    public GameObject InGameUI;
    SceneSelection nowScene = SceneSelection.None;

    public StartAdditionalUI StartAdditionalUI;

    public WorldUIIcons WorldUIIcons;

    private void Awake()
    {
        WorldUIIcons = gameObject.GetComponent<WorldUIIcons>();
    }

    private void Start()
    {
        GameManager.Instance.UIManager = this;
    }

    public void ChangeScene(SceneSelection _afterscene)
    {
        if(_afterscene == nowScene) return;
        nowScene = _afterscene;

        switch (nowScene)
        {
            case SceneSelection.StartScene:
                StartScene.SetActive(true);
                InGameUI.SetActive(false);
                break;

            case SceneSelection.InGameUI:
                StartScene.SetActive(false);
                InGameUI.SetActive(true);
                SelectionUI.Onit();
                break;
        }
    }
}
