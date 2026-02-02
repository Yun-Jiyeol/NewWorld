using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveColor : MonoBehaviour, IPointerClickHandler
{
    Color savecolor = Color.white;
    public Image image;
    public TextMeshProUGUI textMeshPro;

    public void Setting(int num)
    {
        textMeshPro.text = num.ToString();
    }

    public void ChangeColor(int num)
    {
        GameManager.Instance.BuildManager.CheckSave();
        Color color = GameManager.Instance.BuildManager.SaveColors[num];
        savecolor = color;
        image.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.BuildManager.SetColor(savecolor);
    }
}
