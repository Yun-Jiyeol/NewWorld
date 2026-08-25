using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAndBlock
{
    public ChunkCode world;
    public Vector3 blockposition;
    public Color blockcolor;
}

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

    public float maxFrameTime = 3f;
    Queue<WorldAndBlock> buildQueue = new();

    private void Awake()
    {
        StartCoroutine(BuildRoutine());
    }

    public void RequestBuild(List<WorldAndBlock> requests)
    {
        foreach (var r in requests) buildQueue.Enqueue(r);
    }

    public void RequestBuild(WorldAndBlock requests)
    {
        buildQueue.Enqueue(requests);
    }

    public int Checkcountofbuild()
    {
        return buildQueue.Count;
    }

    IEnumerator BuildRoutine()
    {
        while (true)
        {
            float start = Time.realtimeSinceStartup;

            while (buildQueue.Count > 0)
            {
                BuildBlock(buildQueue.Dequeue());

                if ((Time.realtimeSinceStartup - start) * 1000f > maxFrameTime)
                    break;
            }

            yield return null;
        }
    }

    void BuildBlock(WorldAndBlock _wab)
    {
        Color _color = _wab.blockcolor;

        _wab.world.MakeBlockData(_wab.blockposition, _color);
    }

    public void AddPreColor(Color _color)
    {
        if (PreColors.Count == 0)  for (int i = 0; i < precolornum; i++) PreColors.Add(new Color(255, 255, 255));

        if (PreColors.Contains(_color)) return;
        PreColors.Add(_color);

        if (PreColors.Count > precolornum) PreColors.RemoveAt(0);

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

    public void SetColor(Color _color)
    {
        alfar = Mathf.Clamp01(_color.a);

        _color.a = 1f;
        MainColor.color = _color;

        RefreshColorUIIfOpen();
    }

    public Color GetCurrentBuildColor()
    {
        Color color = MainColor.color;
        color.a = alfar;
        return color;
    }

    public void SetColorFromSelectedBlock(Color color)
    {
        SetColor(color);
    }

    public void RefreshColorUIIfOpen()
    {
        if (paretteUI == null)
            paretteUI = FindColor();

        if (paretteUI == null)
            return;

        if (!paretteUI.gameObject.activeInHierarchy)
            return;

        paretteUI.OnSetting();
    }

    public void ApplyCurrentColorToSelectedDestroyBlocks()
    {
        Color color = GetCurrentBuildColor();

        GameManager.Instance.InputManager.ChangeSelectedDestroyBlocksColor(color);
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
