using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreColorUI : MonoBehaviour
{
    public List<GameObject> precolors;
    public void Setting()
    {
        ChangePreColors();
    }

    public void ChangePreColors()
    {
        int i = 3;

        foreach(GameObject precolor in precolors)
        {
            precolor.GetComponent<PreColor>().OnSetting(i);
            i--;
        }
    }
}
