using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        GameManager.Instance.UIManager = this;
    }
}
