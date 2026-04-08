using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldSaveFile : MonoBehaviour
{
    public TextMeshProUGUI WorldName;
    string Name;

    public GameObject Check;
    public bool isCheck = false;
 
    public void Onit(string _name)
    {
        Name = _name;
        WorldName.text = Name;

        if(GameManager.Instance.NowWorldSaveFolder == _name)
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
    }
}
