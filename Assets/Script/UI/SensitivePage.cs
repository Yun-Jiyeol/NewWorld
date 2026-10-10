using UnityEngine;

public class SensitivePage : NeedToDoOnit
{
    public SensitiveControl PlayerMoveControl;
    public SensitiveControl PlayerRotationControl;
    public SensitiveControl ObjectMoveControl;
    public SensitiveControl ObjectRotationControl;
    public SensitiveControl ObjectScaleControl;

    public override void Onit()
    {
        PlayerMoveControl.Onit();
        PlayerRotationControl.Onit();
        ObjectMoveControl.Onit();
        ObjectRotationControl.Onit();
        ObjectScaleControl.Onit();
    }
}
