using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSaveFile : MonoBehaviour
{
    public TextMeshProUGUI WorldName;
    string Name;

    public TextMeshProUGUI WorldMakeDay;

    public Image WorldIcon;

    public GameObject Check;
    public bool isCheck = false;
 
    public void Onit(string _name)
    {
        Name = _name;

        string[] parts = Name.Split('_');

        WorldName.text = parts[0];

        WorldMakeDay.text = "날짜 : " + parts[1].Substring(0, 4) + "년 " 
            + parts[1].Substring(4, 2) + "월 " 
            + parts[1].Substring(6, 2) + "일";

        WorldIcon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[int.Parse(parts[2])];

        if (GameManager.Instance.NowWorldSaveFolder == Name)
        {
            OnCheck();
        } 
    }

    public void ClickWorld()
    {
        if (isCheck)
        {
            UnCheck();
        }
        else
        {
            OnCheck();
        }

        GameManager.Instance.UIManager.StartAdditionalUI.NowUI.GetComponent<LoadWorldUI>().CheckAllWorlds(this);
    }

    public void UnCheck()
    {
        isCheck = false;
        Check.SetActive(false);
    }

    public void OnCheck()
    {
        isCheck = true;
        Check.SetActive(true);
    }

    public string GetName()
    {
        return Name;
    }
}
