using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseHowToMake : BasicUI
{
    int x = 3;
    int y = 3;
    int z = 3;

    int maxValue = 128;

    public TMP_InputField Xtext;
    public TMP_InputField Ytext;
    public TMP_InputField Ztext;

    public ScrollbarHelper XScrollbar;
    public ScrollbarHelper YScrollbar;
    public ScrollbarHelper ZScrollbar;

    bool canchangevalue = true;

    GameObject spawnedimage = null;

    public override void OnSetting()
    {
        if(GameManager.Instance.InputManager.TargetGameObject == null) return;
        Vector3Int TS = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>().GiveThreeSize();

        x = TS.x;
        y = TS.y;
        z = TS.z;

        Xtext.text = x.ToString();
        Ytext.text = y.ToString();
        Ztext.text = z.ToString();

        XScrollbar.Onit(this);
        YScrollbar.Onit(this);
        ZScrollbar.Onit(this);

        check();
    }

    public void check()
    {
        if(GameManager.Instance.BuildManager.Checkcountofbuild() == 0)
        {
            ChangeboolValue(true);
        }
        else
        {
            ChangeboolValue(false);
        }
    }

    void ChangeboolValue(bool iscan)
    {
        canchangevalue = iscan;

        if (canchangevalue)
        {
            if (spawnedimage != null)
            {
                Destroy(spawnedimage);
                spawnedimage = null;
            }
        }
        else
        {
            if(spawnedimage == null)
            {
                spawnedimage = Instantiate(GameManager.Instance.UIManager.BlockImage, gameObject.transform);
            }
        }
    }

    public void ChageXValue()
    {
        int value = 0;
        if (!int.TryParse(Xtext.text, out value))
        {
            Xtext.text = x.ToString();
            return;
        }
        if (value <= 0)
        {
            Xtext.text = x.ToString();
            return;
        }else if(value > maxValue)
        {
            value = maxValue;
        }

        x = value;
        Xtext.text = x.ToString();
        XScrollbar.ChangedValue(x);
        MakeNewWorld();
    }

    public void ChangeXValue(int i)
    {
        int value = x + i;

        if (value <= 0)
        {
            Xtext.text = x.ToString();
            return;
        }
        else if (value > maxValue)
        {
            value = maxValue;
        }

        x = value;
        Xtext.text = x.ToString();
        XScrollbar.ChangedValue(x);
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
        if (value <= 0)
        {
            Ytext.text = y.ToString();
            return;
        }
        else if (value > maxValue)
        {
            value = maxValue;
        }

        y = value;
        Ytext.text = y.ToString();
        YScrollbar.ChangedValue(y);
        MakeNewWorld();
    }

    public void ChangeYValue(int i)
    {
        int value = y + i;

        if (value <= 0)
        {
            Ytext.text = y.ToString();
            return;
        }
        else if (value > maxValue)
        {
            value = maxValue;
        }

        y = value;
        Ytext.text = y.ToString();
        YScrollbar.ChangedValue(y);
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
        if (value <= 0)
        {
            Ztext.text = z.ToString();
            return;
        }
        else if (value > maxValue)
        {
            value = maxValue;
        }

        z = value;
        Ztext.text = z.ToString();
        ZScrollbar.ChangedValue(z);
        MakeNewWorld();
    }

    public void ChangeZValue(int i)
    {
        int value = z + i;

        if (value <= 0)
        {
            Ztext.text = z.ToString();
            return;
        }
        else if (value > maxValue)
        {
            value = maxValue;
        }

        z = value;
        Ztext.text = z.ToString();
        ZScrollbar.ChangedValue(z);
        MakeNewWorld();
    }

    void MakeNewWorld()
    {
        ChangeboolValue(false);

        ChunkCode _world = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>();

        _world.MakeNewWorld(x, y, z);
    }

    public void DestroyWorld()
    {
        GameManager.Instance.UIManager.WorldSeenUI.offScreenDirect();
        GameManager.Instance.WorldManager.DestroyWorld(GameManager.Instance.InputManager.TargetGameObject);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }

    public void ChooseWorld()
    {
        GameManager.Instance.ChangeFlow(GameFlow.WatchOneWorld);
    }
}
