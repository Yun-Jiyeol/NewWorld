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
        if(GameManager.Instance.BuildManager.PreColors.Count == 0)
        {
            for (int i = 0; i < GameManager.Instance.BuildManager.precolornum; i++) GameManager.Instance.BuildManager.PreColors.Add(new Color(255, 255, 255));
        }
        savecolor = GameManager.Instance.BuildManager.PreColors[_num];
        gameObject.GetComponent<Image>().color = savecolor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.BuildManager.SetColor(savecolor);
    }
}
