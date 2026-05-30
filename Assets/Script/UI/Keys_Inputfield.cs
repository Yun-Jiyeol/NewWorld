using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Keys_Inputfield : NeedToDoOnit, IPointerDownHandler
{
    public KeyNameSaves KeyNum;
    TMP_InputField thisInput;
    KeyManager keyManager;
    Coroutine InputCoroutine;
    Keys_KeySettingPage parent;
    UI_BtnEffect btneffect;

    private void Awake()
    {
        thisInput = gameObject.GetComponent<TMP_InputField>();
        btneffect = gameObject.GetComponent<UI_BtnEffect>();
        keyManager = GameManager.Instance.KeyManager;

        thisInput.readOnly = true;
        thisInput.caretWidth = 0;
    }

    private void Start()
    {
        thisInput.onSelect.AddListener(OnSelect);
        thisInput.onDeselect.AddListener(DisSelect);
    }

    public override void GetParent(GameObject _parent)
    {
        parent = _parent.GetComponent<Keys_KeySettingPage>();
    }

    public override void Onit()
    {
        thisInput.text = keyManager.GetKeyFromKeyCode(keyManager.KeyboardDictionary[KeyNum]);
    }

    void OnSelect(string value)
    {
        TrySelect();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TrySelect();
    }

    void TrySelect()
    {
        if (parent.isChange)
        {
            Onit();
            thisInput.DeactivateInputField();
            return;
        }

        if (InputCoroutine != null)
        {
            StopCoroutine(InputCoroutine);
        }

        parent.isChange = true;

        InputCoroutine = StartCoroutine(_InputCoroutine());
    }

    IEnumerator _InputCoroutine()
    {
        yield return null;

        thisInput.text = "";

        Keyboards newKey = new Keyboards();
        btneffect.HoldChange();

        while (true)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                // Shift 제외
                if (key == KeyCode.LeftShift ||
                    key == KeyCode.RightShift)
                {
                    continue;
                }

                if (Input.GetKeyDown(key))
                {
                    // Shift 상태 저장
                    newKey.shiftClick =
                        Input.GetKey(KeyCode.LeftShift) ||
                        Input.GetKey(KeyCode.RightShift);

                    if(keyManager.CheckKeyCode(key) != null)
                    {
                        newKey.key = key;
                        Save(newKey);
                    }
                    else
                    {
                        UnSelect();
                    }

                    yield break;
                }
            }

            yield return null;
        }
    }
    void Save(Keyboards newKey)
    {
        thisInput.DeactivateInputField();
        keyManager.KeyboardDictionary[KeyNum] = newKey;
        parent.isChange = false;
        btneffect.UnHoldChange();
        GameManager.Instance.SaveAndLoadManager.PlayerSetting_SaveKey();
        Onit();
    }

    void UnSelect()
    {
        thisInput.DeactivateInputField();
        parent.isChange = false;
        btneffect.UnHoldChange();
        Onit();
    }

    void DisSelect(string value)
    {

    }
}
