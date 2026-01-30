using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject PreviewWorld;
    public GameObject Compass;
    public SelectionUI SelectionUI;

    private void Start()
    {
        GameManager.Instance.UIManager = this;
    }

    public void OnThePreviewWorld()
    {
        PreviewWorld.SetActive(true);
        GameManager.Instance.MainCamera.SubCamera.SetActive(true);
    }

    public void OffThePreviewWorld()
    {
        PreviewWorld.SetActive(false);
        GameManager.Instance.MainCamera.SubCamera.SetActive(false);
    }
}
