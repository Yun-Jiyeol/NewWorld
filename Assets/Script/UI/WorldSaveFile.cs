using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WorldSaveFile : MonoBehaviour
{
    public TapeSave tapeSave;
    string Name;

    public Image Shadow;

    public bool isCheck = false;
 
    public void Onit(string _name)
    {
        Name = _name;
        string[] parts = Name.Split('_');
        GameObject _tape = Instantiate(GameManager.Instance.MaterialManager.Tapes[int.Parse(parts[3])], gameObject.transform);
        tapeSave = _tape.GetComponent<TapeSave>();

        tapeSave.WorldName.text = parts[0];

        tapeSave.WorldMakeYear.text = parts[1].Substring(0, 4) + '.';
        tapeSave.WorldMakeMonth.text = parts[1].Substring(4, 2) + '.';
        tapeSave.WorldMakeDay.text = parts[1].Substring(6, 2);

        tapeSave.WorldNum.text = (char)('A' + int.Parse(parts[2])) 
            + parts[1].Substring(12, 1) + parts[1].Substring(10, 1) + parts[1].Substring(8, 1);

        tapeSave.WorldIcon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[int.Parse(parts[2])];
        if(tapeSave.Check != null)
            tapeSave.Check.GetComponent<Image>().sprite = tapeSave.WorldIcon.sprite;

        UnCheck();
        if (GameManager.Instance.NowWorldSaveFolder == Name)
        {
            OnCheck();
        } 
    }

    public int TapeInOnit(string _name)
    {
        Name = _name;
        string[] parts = Name.Split('_');
        GameObject _tape = Instantiate(GameManager.Instance.MaterialManager.InsertTapes[int.Parse(parts[3])], gameObject.transform);
        tapeSave = _tape.GetComponent<TapeSave>();

        tapeSave.WorldName.text = parts[0];

        tapeSave.WorldMakeYear.text = parts[1].Substring(0, 4) + '.';
        tapeSave.WorldMakeMonth.text = parts[1].Substring(4, 2) + '.';
        tapeSave.WorldMakeDay.text = parts[1].Substring(6, 2);

        tapeSave.WorldNum.text = (char)('A' + int.Parse(parts[2]))
            + parts[1].Substring(12, 1) + parts[1].Substring(10, 1) + parts[1].Substring(8, 1);

        tapeSave.WorldIcon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[int.Parse(parts[2])];

        return int.Parse(parts[3]);
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
        tapeSave.Check.SetActive(false);
    }

    public void OnCheck()
    {
        isCheck = true;
        tapeSave.Check.SetActive(true);

        GameManager.Instance.SoundManager.PlayEffect(CassetteLiftClackSoundFactory.CreateCassetteLiftClack_05_ClearCassetteClack());
    }

    public string GetName()
    {
        return Name;
    }
}
