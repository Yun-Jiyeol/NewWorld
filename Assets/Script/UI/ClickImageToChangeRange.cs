using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickImageToChangeRange : MonoBehaviour, IPointerClickHandler
{
    LightUI ui;
    public int ChagedValue;

    public void Setting(LightUI _lightui)
    {
        ui = _lightui;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ui.ChangeSettingLightRange(ChagedValue);
    }
}
