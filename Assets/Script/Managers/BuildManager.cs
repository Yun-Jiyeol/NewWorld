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

    ParetteUI paretteUI = null;

    public void AddPreColor(Color _color)
    {
        if (PreColors.Contains(_color)) return;
        PreColors.Add(_color);

        if (PreColors.Count > 4) PreColors.RemoveAt(0);

        if (paretteUI == null) paretteUI = FindColor();
        paretteUI._preColorUI.ChangePreColors();
    }

    ParetteUI FindColor()
    {
        foreach(var want in GameManager.Instance.UIManager.SelectionUI.uIBtnsSelectedUIMaterials)
        {
            if(want.UIenum == SelectionUIenum.Color) return want.UISelected.GetComponent<ParetteUI>();
        }

        return null;
    }

    public void AddSaveColor(Color _color)
    {
        if (PreColors.Contains(_color)) return;
        PreColors.Add(_color);

        if (PreColors.Count > 4) PreColors.RemoveAt(0);
    }

    public void SetColor(Color _color)
    {
        alfar = _color.a;
        _color.a = 1;
        MainColor.color = _color;

        if (paretteUI == null) paretteUI = FindColor();
        paretteUI.OnSetting();
    }
}
