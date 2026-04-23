using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickImageToChangeRange : MonoBehaviour, IPointerDownHandler
{
    LightUI ui;
    public float ChagedValue;

    public void Setting(LightUI _lightui)
    {
        ui = _lightui;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(Changelightrange());
    }

    IEnumerator Changelightrange()
    {
        while (true)
        {
            if (!Input.GetMouseButton(0)) break;

            gameObject.GetComponent<Image>().color = new Color(0.8f, 0.8f, 1f);
            ui.ChangeSettingLightRange(ChagedValue * Time.deltaTime);
            yield return null;
        }

        gameObject.GetComponent<Image>().color = Color.white;
    }
}
