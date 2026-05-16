using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Keyboards
{
    public KeyCode key;

    public bool shiftClick = false;
}

public enum KeyNameSaves
{
    // OnPlayerMove
    PFrontMove,
    PLeftMove,
    PBackMove,
    PRightMove,

    // OnWorldMove
    PWBaseMoveKey,
    WUpMove,
    WLeftMove,
    WDownMove,
    WRightMove,
    WFrontMove,
    WBackMove, //9

    // OnSelect
    SelectBtn,
    AdditionalSelectBtn,
    BuildCube,
    ChooseBtn,
    DestroyBlockKey,
    LockSelectionKey,

    // OnUISelect
    SpawnWorld,
    PopUpUI,

    // OnSaveColor
    firstsave,
    secondsave,
    thirdsave,
    forthsave,
    fifthsave,
    sixthsave,

    // OnLoadColor
    firstload,
    secondload,
    thirdload,
    forthload,
    fifthload,
    sixthload,

    // 월드 세부 조정 키
    WorldTurnTrigger,
    TurnXPlus,
    TurnXMinus,
    TurnYPlus,
    TurnYMinus,
    TurnZPlus,
    TurnZMinus,

    // 카메라 회전
    CameraTurn
}

public class KeyManager : MonoBehaviour
{
    public Dictionary<KeyNameSaves, Keyboards> KeyboardDictionary = new Dictionary<KeyNameSaves, Keyboards>()
    { 
        // OnPlayerMove
        { KeyNameSaves.CameraTurn, new Keyboards() { key = KeyCode.F12 } },
        { KeyNameSaves.PFrontMove, new Keyboards() { key = KeyCode.W } },
        { KeyNameSaves.PLeftMove, new Keyboards() { key = KeyCode.A } },
        { KeyNameSaves.PBackMove, new Keyboards() { key = KeyCode.S } },
        { KeyNameSaves.PRightMove, new Keyboards() { key = KeyCode.D } },

        // OnWorldMove
        { KeyNameSaves.PWBaseMoveKey, new Keyboards() { key = KeyCode.Mouse0 } },
        { KeyNameSaves.WUpMove, new Keyboards() { key = KeyCode.E } },
        { KeyNameSaves.WLeftMove, new Keyboards() { key = KeyCode.A } },
        { KeyNameSaves.WDownMove, new Keyboards() { key = KeyCode.Q } },
        { KeyNameSaves.WRightMove, new Keyboards() { key = KeyCode.D } },
        { KeyNameSaves.WFrontMove, new Keyboards() { key = KeyCode.W } },
        { KeyNameSaves.WBackMove, new Keyboards() { key = KeyCode.S } },
        
        // OnWorldTurn
        { KeyNameSaves.WorldTurnTrigger, new Keyboards() { key = KeyCode.Comma } },
        { KeyNameSaves.TurnXPlus, new Keyboards() { key = KeyCode.LeftBracket } },
        { KeyNameSaves.TurnXMinus, new Keyboards() { key = KeyCode.RightBracket } },
        { KeyNameSaves.TurnYPlus, new Keyboards() { key = KeyCode.Semicolon } },
        { KeyNameSaves.TurnYMinus, new Keyboards() { key = KeyCode.Quote } },
        { KeyNameSaves.TurnZPlus, new Keyboards() { key = KeyCode.Period } },
        { KeyNameSaves.TurnZMinus, new Keyboards() { key = KeyCode.Slash } },

        // OnSelect
        { KeyNameSaves.SelectBtn, new Keyboards() { key = KeyCode.Mouse0 } },
        { KeyNameSaves.AdditionalSelectBtn, new Keyboards() { key = KeyCode.Mouse0, shiftClick = true } },
        { KeyNameSaves.BuildCube, new Keyboards() { key = KeyCode.Mouse1 } },
        { KeyNameSaves.ChooseBtn, new Keyboards() { key = KeyCode.Mouse2 } },
        { KeyNameSaves.DestroyBlockKey, new Keyboards() { key = KeyCode.Delete } },
        { KeyNameSaves.LockSelectionKey, new Keyboards() { key = KeyCode.LeftControl } },

        // OnUISelect
        { KeyNameSaves.SpawnWorld, new Keyboards() { key = KeyCode.P } },
        { KeyNameSaves.PopUpUI, new Keyboards() { key = KeyCode.Tab } },

        // OnSaveColor
        { KeyNameSaves.firstsave, new Keyboards() { key = KeyCode.Alpha1, shiftClick = true } },
        { KeyNameSaves.secondsave, new Keyboards() { key = KeyCode.Alpha2, shiftClick = true } },
        { KeyNameSaves.thirdsave, new Keyboards() { key = KeyCode.Alpha3, shiftClick = true } },
        { KeyNameSaves.forthsave, new Keyboards() { key = KeyCode.Alpha4, shiftClick = true } },
        { KeyNameSaves.fifthsave, new Keyboards() { key = KeyCode.Alpha5, shiftClick = true } },
        { KeyNameSaves.sixthsave, new Keyboards() { key = KeyCode.Alpha6, shiftClick = true } },

        // OnLoadColor
        { KeyNameSaves.firstload, new Keyboards() { key = KeyCode.Alpha1 } },
        { KeyNameSaves.secondload, new Keyboards() { key = KeyCode.Alpha2 } },
        { KeyNameSaves.thirdload, new Keyboards() { key = KeyCode.Alpha3 } },
        { KeyNameSaves.forthload, new Keyboards() { key = KeyCode.Alpha4 } },
        { KeyNameSaves.fifthload, new Keyboards() { key = KeyCode.Alpha5 } },
        { KeyNameSaves.sixthload, new Keyboards() { key = KeyCode.Alpha6 } },
    };

    public bool isGetKeyDown(Keyboards _chooseKey)
    {
        if (_chooseKey.shiftClick)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }
        }

        if (Input.GetKeyDown(_chooseKey.key))
        {
            return true;
        }
        return false;
    }

    public bool isGetKeyUp(Keyboards _chooseKey)
    {
        if (_chooseKey.shiftClick)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }
        }

        if (Input.GetKeyUp(_chooseKey.key))
        {
            return true;
        }

        return false;
    }

    public bool isGetKey(Keyboards _chooseKey)
    {
        if (_chooseKey.shiftClick)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }
        }

        if (Input.GetKey(_chooseKey.key))
        {
            return true;
        }

        return false;
    }

    public string GetKeyFromKeyCode(Keyboards keyboard)
    {  // A ~ Z
        string output = "";
        if (keyboard.shiftClick)
        {
            output = "Shift + ";
        }

        KeyCode key = keyboard.key;

        if (key >= KeyCode.A && key <= KeyCode.Z)
        {
            return output + key.ToString();
        }

        // 0 ~ 9
        if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
        {
            return output + key.ToString().Replace("Alpha", "");
        }

        // F1 ~ F12
        if (key >= KeyCode.F1 && key <= KeyCode.F12)
        {
            return output + key.ToString();
        }

        switch (key)
        {
            // 마우스
            case KeyCode.Mouse0:
                return output + "왼쪽 클릭";

            case KeyCode.Mouse1:
                return output + "오른쪽 클릭";

            case KeyCode.Mouse2:
                return output + "휠 클릭";

            // 특수키
            case KeyCode.Tab:
                return output + "Tab";

            case KeyCode.LeftShift:
                return output + "LeftShift";
            case KeyCode.RightShift:
                return output + "RightShift";

            case KeyCode.LeftControl:
                return output + "LeftControl";
            case KeyCode.RightControl:
                return output + "RightControl";

            case KeyCode.LeftAlt:
                return output + "LeftAlt";
            case KeyCode.RightAlt:
                return output + "RightAlt";

            case KeyCode.Space:
                return output + "Space";

            // 방향키
            case KeyCode.UpArrow:
                return output + "위 화살표";

            case KeyCode.DownArrow:
                return output + "아래 화살표";

            case KeyCode.LeftArrow:
                return output + "왼쪽 화살표";

            case KeyCode.RightArrow:
                return output + "오른쪽 화살표";

            case KeyCode.LeftBracket:
                return output + "[";

            case KeyCode.RightBracket:
                return output + "]";

            case KeyCode.Semicolon:
                return output + ";";

            case KeyCode.Quote:
                return output + "'";

            case KeyCode.Period:
                return output + ".";

            case KeyCode.Slash:
                return output + "/";

            case KeyCode.Comma:
                return output + ",";
        }

        // 기본값
        return output + key.ToString();
    }

    public KeyCode? CheckKeyCode(KeyCode key)
    {
        // A ~ Z
        if (key >= KeyCode.A &&
            key <= KeyCode.Z)
        {
            return key;
        }

        // 0 ~ 9
        if (key >= KeyCode.Alpha0 &&
            key <= KeyCode.Alpha9)
        {
            return key;
        }

        // F1 ~ F12
        if (key >= KeyCode.F1 &&
            key <= KeyCode.F12)
        {
            return key;
        }

        switch (key)
        {
            // 방향키
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:

            // 특수키
            case KeyCode.Tab:
            case KeyCode.Space:
            case KeyCode.LeftControl:
            case KeyCode.RightControl:
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:

            // 기호
            case KeyCode.Comma:
            case KeyCode.Period:
            case KeyCode.Slash:
            case KeyCode.Semicolon:
            case KeyCode.Quote:
            case KeyCode.LeftBracket:
            case KeyCode.RightBracket:

            // 마우스
            case KeyCode.Mouse0:
            case KeyCode.Mouse1:
            case KeyCode.Mouse2:

                return key;
        }

        // 허용되지 않은 키 눌림
        return null;
    }
}
