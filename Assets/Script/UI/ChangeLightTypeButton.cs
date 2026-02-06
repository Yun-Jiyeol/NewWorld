using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeLightTypeButton : MonoBehaviour, IPointerClickHandler
{
    public LightType _lighttype;
    LightUI ui;

    public void Setting(LightUI _lightui)
    {
        ui = _lightui;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        ui.ChangeType(_lighttype);
    }
}
