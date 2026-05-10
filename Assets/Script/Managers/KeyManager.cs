using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keyboards
{
    public KeyCode key;

    public bool shiftClick = false;
}

public class KeyManager : MonoBehaviour
{
    [Header("OnPlayerMove")]
    public Keyboards PFrontMove = new Keyboards() { key = KeyCode.W };
    public Keyboards PLeftMove = new Keyboards() { key = KeyCode.A };
    public Keyboards PBackMove = new Keyboards() { key = KeyCode.S };
    public Keyboards PRightMove = new Keyboards() { key = KeyCode.D };

    [Header("OnWorldMove")]
    public Keyboards WUpMove = new Keyboards() { key = KeyCode.W };
    public Keyboards WLeftMove = new Keyboards() { key = KeyCode.A };
    public Keyboards WDownMove = new Keyboards() { key = KeyCode.S };
    public Keyboards WRightMove = new Keyboards() { key = KeyCode.D };
    public Keyboards WFrontMove = new Keyboards() { key = KeyCode.E };
    public Keyboards WBackMove = new Keyboards() { key = KeyCode.Q };

    [Header("OnSelect")]
    public Keyboards SelectBtn = new Keyboards() { key = KeyCode.Mouse0 };
    public Keyboards AdditionalSelectBtn = new Keyboards() { key = KeyCode.Mouse0 , shiftClick = true};
    public Keyboards BuildCube = new Keyboards() { key = KeyCode.Mouse1 };
    public Keyboards ChooseBtn = new Keyboards() { key = KeyCode.Mouse2 };
    public Keyboards DestroyBlockKey = new Keyboards() { key = KeyCode.Delete };
    public Keyboards LockSelectionKey = new Keyboards() { key = KeyCode.LeftControl };

    [Header("OnUISelect")]
    public Keyboards SpawnWorld = new Keyboards() { key = KeyCode.P };
    public Keyboards PopUpUI = new Keyboards() { key = KeyCode.Tab };

    [Header("OnSaveColor")]
    public Keyboards firstsave = new Keyboards() { key = KeyCode.Alpha1 , shiftClick = true };
    public Keyboards secondsave = new Keyboards() { key = KeyCode.Alpha2, shiftClick = true };
    public Keyboards thirdsave = new Keyboards() { key = KeyCode.Alpha3, shiftClick = true };
    public Keyboards forthsave = new Keyboards() { key = KeyCode.Alpha4, shiftClick = true };
    public Keyboards fifthsave = new Keyboards() { key = KeyCode.Alpha5, shiftClick = true };
    public Keyboards sixthsave = new Keyboards() { key = KeyCode.Alpha6, shiftClick = true };

    [Header("OnLoadColor")]
    public Keyboards firstload = new Keyboards() { key = KeyCode.Alpha1};
    public Keyboards secondload = new Keyboards() { key = KeyCode.Alpha2};
    public Keyboards thirdload = new Keyboards() { key = KeyCode.Alpha3 };
    public Keyboards forthload = new Keyboards() { key = KeyCode.Alpha4};
    public Keyboards fifthload = new Keyboards() { key = KeyCode.Alpha5 };
    public Keyboards sixthload = new Keyboards() { key = KeyCode.Alpha6};

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
}
