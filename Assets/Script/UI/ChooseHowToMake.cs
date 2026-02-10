using TMPro;
using UnityEditor.UI;

public class ChooseHowToMake : BasicUI
{
    int x = 3;
    int y = 3;
    int z = 3;

    public TMP_InputField Xtext;
    public TMP_InputField Ytext;
    public TMP_InputField Ztext;

    public override void OnSetting()
    {
        x = 3;
        y = 3;
        z = 3;

        Xtext.text = x.ToString();
        Ytext.text = y.ToString();
        Ztext.text = z.ToString();
    }

    public void ChageXValue()
    {
        int value = 0;
        if (!int.TryParse(Xtext.text, out value))
        {
            Xtext.text = x.ToString();
            return;
        }
        if (value <= 0 || value > 100)
        {
            Xtext.text = x.ToString();
            return;
        }

        x = value;
        MakeNewWorld();
    }

    public void ChangeXValue(int i)
    {
        int value = x + i;

        if (value <= 0 || value > 100)
        {
            Xtext.text = x.ToString();
            return;
        }

        x = value;
        Xtext.text = x.ToString();
        MakeNewWorld();
    }

    public void ChageYValue()
    {
        int value = 0;
        if (!int.TryParse(Ytext.text, out value))
        {
            Ytext.text = y.ToString();
            return;
        }
        if (value <= 0 || value > 100)
        {
            Ytext.text = y.ToString();
            return;
        }

        y = value;
        MakeNewWorld();
    }

    public void ChangeYValue(int i)
    {
        int value = y + i;

        if (value <= 0 || value > 100)
        {
            Ytext.text = y.ToString();
            return;
        }

        y = value;
        Ytext.text = y.ToString();
        MakeNewWorld();
    }

    public void ChageZValue()
    {
        int value = 0;
        if (!int.TryParse(Ztext.text, out value))
        {
            Ztext.text = z.ToString();
            return;
        }
        if (value <= 0 || value > 100)
        {
            Ztext.text = z.ToString();
            return;
        }

        z = value;
        MakeNewWorld();
    }

    public void ChangeZValue(int i)
    {
        int value = z + i;

        if (value <= 0 || value > 100)
        {
            Ztext.text = z.ToString();
            return;
        }

        z = value;
        Ztext.text = z.ToString();
        MakeNewWorld();
    }

    void MakeNewWorld()
    {
        WorldBaseCode _world = GameManager.Instance.InputManager.TargetGameObject.GetComponent<WorldBaseCode>();

        _world.MakeNewWorld(x, y, z);
    }

    public void DestroyWorld()
    {
        GameManager.Instance.WorldManager.DestroyWorld(GameManager.Instance.InputManager.TargetGameObject);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }

    public void ChooseWorld()
    {
        GameManager.Instance.ChangeFlow(GameFlow.WatchOneWorld);
    }
}
