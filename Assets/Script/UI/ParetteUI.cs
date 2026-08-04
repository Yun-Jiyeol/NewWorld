using System.Collections;
using UnityEngine;

public class ParetteUI : BasicUI
{
    public PaletteLineRenderer _paletteLineRenderer;
    public PaletteAlfaLine _paletteAlfaLine;
    public FinalChooseColor _finalChooseColor;
    public PreColorUI _preColorUI;
    public SaveUI _saveUI;

    public override void OnSetting()
    {
        StartCoroutine(OnSettingCoroutine());
    }

    private IEnumerator OnSettingCoroutine()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        _paletteLineRenderer.Setting(this);
        _paletteAlfaLine.Setting();
        _finalChooseColor.Setting();
        _preColorUI.Setting();
        _saveUI.Setting();
    }
}