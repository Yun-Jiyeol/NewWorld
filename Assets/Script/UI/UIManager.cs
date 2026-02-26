using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject PreviewWorld;
    public GameObject Compass;
    public SelectionUI SelectionUI;
    public GameObject BlockImage;

    private void Start()
    {
        GameManager.Instance.UIManager = this;
    }
}
