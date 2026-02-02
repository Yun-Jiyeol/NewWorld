using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreColorUI : MonoBehaviour
{
    public GameObject precolorObject;
    List<GameObject> precolors = new List<GameObject>();

    public void Setting()
    {
        makesomeObjects();
        ChangePreColors();
    }

    void makesomeObjects()
    {
        if(precolors.Count != 0) return;

        precolors.Add(precolorObject);
        for(int i = 1; i< GameManager.Instance.BuildManager.precolornum; i++)
        {
            GameObject addcolor = Instantiate(precolorObject,gameObject.transform);
            precolors.Add(addcolor);
        }
    }


    public void ChangePreColors()
    {
        int i = GameManager.Instance.BuildManager.precolornum - 1;

        foreach(GameObject precolor in precolors)
        {
            precolor.GetComponent<PreColor>().OnSetting(i);
            i--;
        }
    }
}
