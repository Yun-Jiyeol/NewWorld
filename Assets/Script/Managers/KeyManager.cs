using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyInputType
{
    Key,
    MouseWheel
}

[System.Serializable]
public class Keyboards
{
    public KeyInputType inputType = KeyInputType.Key;

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
    TurnnJustX,
    TurnnJustY,
    TurnnJustZ,

    // 카메라 회전
    CameraTurn,

    // 월드 확대 / 축소
    // 기존 저장 enum 번호가 밀리지 않도록 항상 맨 뒤에 유지
    WorldScale
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
        { KeyNameSaves.TurnnJustX, new Keyboards() { key = KeyCode.X } },
        { KeyNameSaves.TurnnJustY, new Keyboards() { key = KeyCode.Y } },
        { KeyNameSaves.TurnnJustZ, new Keyboards() { key = KeyCode.Z } },

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

        // OnWorldScale
        { KeyNameSaves.WorldScale, new Keyboards() { inputType = KeyInputType.MouseWheel } },
    };

    bool CanUseInput(Keyboards keyboard)
    {
        if (keyboard == null)
            return false;

        if (keyboard.shiftClick)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                return false;
        }
        else
        {
            // 기존 동작 유지:
            // Shift가 눌린 동안에는 shiftClick=false 입력을 막는다.
            if (Input.GetKey(KeyCode.LeftShift))
                return false;
        }

        return true;
    }

    public float GetInputValue(Keyboards keyboard)
    {
        if (!CanUseInput(keyboard))
            return 0f;

        switch (keyboard.inputType)
        {
            case KeyInputType.MouseWheel:
                // + : Wheel Up
                // - : Wheel Down
                return Input.GetAxis("Mouse ScrollWheel");

            default:
                return Input.GetKey(keyboard.key) ? 1f : 0f;
        }
    }

    public bool isGetKeyDown(Keyboards _chooseKey)
    {
        if (!CanUseInput(_chooseKey))
            return false;

        switch (_chooseKey.inputType)
        {
            case KeyInputType.MouseWheel:
                return Mathf.Abs(
                    Input.GetAxis("Mouse ScrollWheel")
                ) > 0f;

            default:
                return Input.GetKeyDown(_chooseKey.key);
        }
    }

    public bool isGetKeyUp(Keyboards _chooseKey)
    {
        if (!CanUseInput(_chooseKey))
            return false;

        switch (_chooseKey.inputType)
        {
            // 마우스 휠은 "놓는 순간"이라는 개념이 없으므로
            // GetKeyUp은 사용하지 않는다.
            case KeyInputType.MouseWheel:
                return false;

            default:
                return Input.GetKeyUp(_chooseKey.key);
        }
    }

    public bool isGetKey(Keyboards _chooseKey)
    {
        if (_chooseKey == null)
            return false;

        if (_chooseKey.inputType == KeyInputType.MouseWheel)
        {
            return Mathf.Abs(
                GetInputValue(_chooseKey)
            ) > 0f;
        }

        return GetInputValue(_chooseKey) > 0f;
    }

    public string GetKeyFromKeyCode(Keyboards keyboard)
    {
        LanguageEnum nowLanguage = GameManager.Instance.TranslationManager.GetLanguage();

        string output = "";

        if (keyboard.shiftClick)
        {
            output += GetShiftText(nowLanguage);
        }

        if (keyboard.inputType == KeyInputType.MouseWheel)
        {
            return output + GetLanguageText(
                nowLanguage,
                "마우스 휠",
                "Mouse Wheel",
                "0x57484C"
            );
        }

        KeyCode key = keyboard.key;

        // A ~ Z
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

        return output + GetSpecialKeyText(key, nowLanguage);
    }

    string GetShiftText(LanguageEnum language)
    {
        switch (language)
        {
            case LanguageEnum.Kor:
                return "쉬프트 + ";

            case LanguageEnum.Eng:
                return "Shift + ";

            case LanguageEnum.Com:
                return "0x534846 + "; // SHF

            default:
                return "Shift + ";
        }
    }

    string GetSpecialKeyText(KeyCode key, LanguageEnum language)
    {
        switch (key)
        {
            // 마우스
            case KeyCode.Mouse0:
                return GetLanguageText(language, "왼쪽 클릭", "Left Click", "0x4C434B"); // LCK

            case KeyCode.Mouse1:
                return GetLanguageText(language, "오른쪽 클릭", "Right Click", "0x52434B"); // RCK

            case KeyCode.Mouse2:
                return GetLanguageText(language, "휠 클릭", "Wheel Click", "0x57434B"); // WCK


            // 특수키
            case KeyCode.Tab:
                return GetLanguageText(language, "탭", "Tab", "0x544142"); // TAB

            case KeyCode.Delete:
                return GetLanguageText(language, "삭제", "Delete", "0x44454C"); // DELECT

            case KeyCode.LeftShift:
                return GetLanguageText(language, "왼쪽 쉬프트", "Left Shift", "0x4C5F534846"); // L_SHF

            case KeyCode.RightShift:
                return GetLanguageText(language, "오른쪽 쉬프트", "Right Shift", "0x525F534846"); // R_SHF

            case KeyCode.LeftControl:
                return GetLanguageText(language, "왼쪽 컨트롤", "Left Control", "0x4C5F4354524C"); // L_CTRL

            case KeyCode.RightControl:
                return GetLanguageText(language, "오른쪽 컨트롤", "Right Control", "0x525F4354524C"); // R_CTRL

            case KeyCode.LeftAlt:
                return GetLanguageText(language, "왼쪽 알트", "Left Alt", "0x4C5F414C54"); // L_ALT

            case KeyCode.RightAlt:
                return GetLanguageText(language, "오른쪽 알트", "Right Alt", "0x525F414C54"); // R_ALT

            case KeyCode.Space:
                return GetLanguageText(language, "스페이스", "Space", "0x535043"); // SPC


            // 방향키
            case KeyCode.UpArrow:
                return GetLanguageText(language, "위 화살표", "Up Arrow", "0x55505F415252"); // UP_ARR

            case KeyCode.DownArrow:
                return GetLanguageText(language, "아래 화살표", "Down Arrow", "0x444E5F415252"); // DN_ARR

            case KeyCode.LeftArrow:
                return GetLanguageText(language, "왼쪽 화살표", "Left Arrow", "0x4C46545F415252"); // LFT_ARR

            case KeyCode.RightArrow:
                return GetLanguageText(language, "오른쪽 화살표", "Right Arrow", "0x5247545F415252"); // RGT_ARR


            // 기호
            case KeyCode.LeftBracket:
                return "[";

            case KeyCode.RightBracket:
                return "]";

            case KeyCode.Semicolon:
                return ";";

            case KeyCode.Quote:
                return "'";

            case KeyCode.Period:
                return ".";

            case KeyCode.Slash:
                return "/";

            case KeyCode.Comma:
                return ",";

            default:
                return key.ToString();
        }
    }

    string GetLanguageText(LanguageEnum language, string kor, string eng, string com)
    {
        switch (language)
        {
            case LanguageEnum.Kor:
                return kor;

            case LanguageEnum.Eng:
                return eng;

            case LanguageEnum.Com:
                return com;

            default:
                return eng;
        }
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
            case KeyCode.Delete:
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
