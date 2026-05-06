using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldSaveFile : MonoBehaviour
{
    public TextMeshProUGUI WorldName;
    public TextMeshProUGUI WorldNum;
    string Name;

    public TextMeshProUGUI WorldMakeYear;
    public TextMeshProUGUI WorldMakeMonth;
    public TextMeshProUGUI WorldMakeDay;

    public Image WorldIcon;
    public Image Shadow;
    public Image AllImage;

    public GameObject Check;
    public bool isCheck = false;
 
    public void Onit(string _name)
    {
        Name = _name;

        string[] parts = Name.Split('_');

        WorldName.text = parts[0];

        WorldMakeYear.text = parts[1].Substring(0, 4) + '.';
        WorldMakeMonth.text = parts[1].Substring(4, 2) + '.';
        WorldMakeDay.text = parts[1].Substring(6, 2);

        WorldNum.text = (char)('A' + int.Parse(parts[2])) 
            + parts[1].Substring(12, 1) + parts[1].Substring(10, 1) + parts[1].Substring(8, 1);

        WorldIcon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[int.Parse(parts[2])];

        AllImage.sprite = GameManager.Instance.MaterialManager.Tapes[int.Parse(parts[3])];

        if(int.Parse(parts[3]) == 5)
        {
            WorldName.color = Color.white;
            WorldNum.color = Color.white;
            WorldMakeYear.color = Color.white;
            WorldMakeMonth.color = Color.white;
            WorldMakeDay.color = Color.white;
        }

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
