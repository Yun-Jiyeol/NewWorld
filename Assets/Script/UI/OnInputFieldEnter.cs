using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnInputFieldEnter : MonoBehaviour
{
    UI_BtnEffect thisBtnEffect;
    bool hasBtnEffect = true;

    TMP_InputField thisInput;
    bool isSelected = false;

    private void Awake()
    {
        if (!TryGetComponent<TMP_InputField>(out thisInput))
        {
            enabled = false;
            return;
        }

        if (!TryGetComponent<UI_BtnEffect>(out thisBtnEffect))
        {
            hasBtnEffect = false;
        }
    }

    private void Start()
    {
        thisInput.onSelect.AddListener(OnSelected);
        thisInput.onDeselect.AddListener(OnDeselected);
        thisInput.onSubmit.AddListener(OnSubmitted);
    }

    private void Update()
    {
        if (!isSelected) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            DeselectInputField();
        }
    }

    void OnSelected(string value)
    {
        if (isSelected) return;

        isSelected = true;

        if (hasBtnEffect)
            thisBtnEffect.HoldChange();

        GameManager.Instance.SoundManager.PlayEffect(
            SoundChoose.CallAudio(ClickSound.NameSound)
        );
    }

    void OnSubmitted(string value)
    {
        DeselectInputField();
    }

    void OnDeselected(string value)
    {
        EndSelectState();
    }

    void DeselectInputField()
    {
        thisInput.DeactivateInputField();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        EndSelectState();
    }

    void EndSelectState()
    {
        if (!isSelected) return;

        isSelected = false;

        if (hasBtnEffect)
            thisBtnEffect.UnHoldChange();
    }

    private void OnDisable()
    {
        EndSelectState();
    }

    private void OnDestroy()
    {
        if (thisInput == null) return;

        thisInput.onSelect.RemoveListener(OnSelected);
        thisInput.onDeselect.RemoveListener(OnDeselected);
        thisInput.onSubmit.RemoveListener(OnSubmitted);
    }
}