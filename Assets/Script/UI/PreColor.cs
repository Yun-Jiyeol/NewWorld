using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreColor : MonoBehaviour
{
    Color savecolor;

    public void OnSetting(int _num)
    {
        savecolor = GameManager.Instance.BuildManager.PreColors[_num];
        gameObject.GetComponent<Image>().color = savecolor;
    }
}
