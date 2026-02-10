using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveUI : MonoBehaviour
{
    public GameObject savecolor;
    List<GameObject> SaveColors = new List<GameObject>();

    public void Setting()
    {
        MakeSaveColors();
        SettingAllColor();
    }

    void MakeSaveColors()
    {
        if(SaveColors.Count != 0) return;

        SaveColors.Add(savecolor);
        savecolor.GetComponent<SaveColor>().Setting(1);

        for (int i = 1; i < GameManager.Instance.BuildManager.savecolornum; i++)
        {
            GameObject go = Instantiate(savecolor, gameObject.transform);
            go.GetComponent<SaveColor>().Setting(i+1);
            SaveColors.Add(go);
        }
    }

    void SettingAllColor()
    {
        for(int i = 0; i< SaveColors.Count; i++)
        {
            ChangeColor(i);
        }
    }

    public void ChangeColor(int i)
    {
        MakeSaveColors();

        SaveColors[i].GetComponent<SaveColor>().ChangeColor(i);
    }
}
