using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public GameObject BasicBlock;

    public GameObject ChooseBlock;

    public Material MainColor;
    public float alfar = 1;

    public List<Color> PreColors;
    public List<Color> SaveColors;

    public void AddPreColor(Color _color)
    {
        if (PreColors.Contains(_color)) return;
        PreColors.Add(_color);

        if (PreColors.Count > 4) PreColors.RemoveAt(0);

        GameObject colorui = FindColor();
        if (colorui == null) Debug.Log("Error");

        colorui.GetComponent<ParetteUI>()._preColorUI.ChangePreColors();
    }

    GameObject FindColor()
    {
        foreach(var want in GameManager.Instance.UIManager.SelectionUI.uIBtnsSelectedUIMaterials)
        {
            if(want.UIenum == SelectionUIenum.Color) return want.UISelected;
        }

        return null;
    }

    public void AddSaveColor(Color _color)
    {
        if (PreColors.Contains(_color)) return;
        PreColors.Add(_color);

        if (PreColors.Count > 4) PreColors.RemoveAt(0);
    }
}
