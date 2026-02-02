using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PreColor : MonoBehaviour, IPointerClickHandler
{
    Color savecolor;

    public void OnSetting(int _num)
    {
        savecolor = GameManager.Instance.BuildManager.PreColors[_num];
        gameObject.GetComponent<Image>().color = savecolor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.BuildManager.SetColor(savecolor);
    }
}
