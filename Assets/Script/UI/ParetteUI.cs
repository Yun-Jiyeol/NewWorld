public class ParetteUI : BasicUI
{
    public PaletteLineRenderer _paletteLineRenderer;
    public PaletteAlfaLine _paletteAlfaLine;
    public FinalChooseColor _finalChooseColor;

    public override void OnSetting()
    {
        _paletteLineRenderer.Setting(this);
        _paletteAlfaLine.Setting();
        _finalChooseColor.Setting();
    }
}
