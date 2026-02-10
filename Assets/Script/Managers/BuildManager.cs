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

    public int precolornum = 12;
    public int savecolornum = 6;

    ParetteUI paretteUI = null;
    SaveUI saveUI = null;

    public void AddPreColor(Color _color)
    {
        if (PreColors.Count == 0) for (int i = 0; i < precolornum; i++) PreColors.Add(new Color(255, 255, 255));

        if (PreColors.Contains(_color)) return;
        PreColors.Add(_color);

        if (PreColors.Count > precolornum) PreColors.RemoveAt(0);

        if (paretteUI == null) paretteUI = FindColor();
        paretteUI._preColorUI.ChangePreColors();

        GameManager.Instance.SaveAndLoadManager.SaveData();
    }

    ParetteUI FindColor()
    {
        foreach(var want in GameManager.Instance.UIManager.SelectionUI.uIBtnsSelectedUIMaterials)
        {
            if(want.UIenum == SelectionUIenum.Color) return want.UISelected.GetComponent<ParetteUI>();
        }

        return null;
    }

    public void SetColor(Color _color)
    {
        alfar = _color.a;
        _color.a = 1;
        MainColor.color = _color;

        if (paretteUI == null) paretteUI = FindColor();
        paretteUI.OnSetting();
    }

    public void CheckSave()
    {
        if (SaveColors.Count == 0) for (int i = 0; i < savecolornum; i++) SaveColors.Add(new Color(255, 255, 255));
    }

    public void AddSaveColor(int _num)
    {
        CheckSave();
        if (saveUI == null) saveUI = FindSave();

        Color _color = MainColor.color;
        _color.a = alfar;
        SaveColors[_num] = _color;

        saveUI.ChangeColor(_num);
        GameManager.Instance.SaveAndLoadManager.SaveData();
    }

    public void AddSaveColor(Color color, int _num)
    {
        CheckSave();
        if (saveUI == null) saveUI = FindSave();

        Color _color = color;
        SaveColors[_num] = _color;

        saveUI.ChangeColor(_num);
    }

    SaveUI FindSave()
    {
        foreach (var want in GameManager.Instance.UIManager.SelectionUI.uIBtnsSelectedUIMaterials)
        {
            if (want.UIenum == SelectionUIenum.Color) return want.UISelected.GetComponent<ParetteUI>()._saveUI;
        }

        return null;
    }
}
