using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldSaveFile : MonoBehaviour
{
    public TextMeshProUGUI WorldName;
    string Name;

    public GameObject Check;
    bool isCheck = false;
 
    public void Onit(string _name)
    {
        Name = _name;
        WorldName.text = Name;
    }

    public void ClickWorld()
    {
        if (isCheck)
        {
            UnCheck();
        }
        else
        {
            OnCheck();
        }
    }

    public void UnCheck()
    {
        isCheck = false;
    }

    public void OnCheck()
    {
        isCheck = true;
        GameManager.Instance.NowWorldSaveFolder = Name;
    }
}
