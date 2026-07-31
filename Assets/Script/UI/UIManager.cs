using DG.Tweening;
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

    public StartSceneUI StartSceneUI;
    public StartAdditionalUI StartAdditionalUI;

    public WorldUIIcons WorldUIIcons;
    public RectTransform _Wha;

    public bool isReady = false;

    private void Awake()
    {
        WorldUIIcons = gameObject.GetComponent<WorldUIIcons>();
    }

    public void Onit()
    {
        isReady = false;

        RectTransform _Com = Compass.GetComponent<RectTransform>();
        RectTransform _Sel = SelectionUI.gameObject.GetComponent<RectTransform>();

        _Com.localScale = Vector2.one * 1.2f;
        _Sel.anchoredPosition = new Vector2(0, 0);
        _Wha.anchoredPosition = new Vector2(0, -120);

        _Com.DOScale(Vector2.one, 1f).OnComplete(() =>
        {
            _Sel.DOAnchorPos(new Vector2(-145f, 0), 0.5f).OnComplete(() =>
            {
                isReady = true;
            });
            _Wha.DOAnchorPos(Vector2.zero, 2f);
        });
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
