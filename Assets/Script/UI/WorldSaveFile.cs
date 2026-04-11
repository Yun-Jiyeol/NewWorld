using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public class WorldSaveFile : MonoBehaviour
{
    public TextMeshProUGUI WorldName;
    string Name;

    public GameObject Check;
    public bool isCheck = false;
 
    public void Onit(string _name)
    {
        Name = _name;

        int index = _name.IndexOf('_');
        if (index > 0)
        {
            _name = _name.Substring(0, index);
        }
        WorldName.text = _name;

        if(GameManager.Instance.NowWorldSaveFolder == Name)
        {
            OnCheck();
        } 
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

        GameManager.Instance.UIManager.StartAdditionalUI.NowUI.GetComponent<LoadWorldUI>().CheckAllWorlds(this);
    }

    public void UnCheck()
    {
        isCheck = false;
        Check.SetActive(false);
    }

    public void OnCheck()
    {
        isCheck = true;
        Check.SetActive(true);
        GameManager.Instance.NowWorldSaveFolder = Name;
    }
}
