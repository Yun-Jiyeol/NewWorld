using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SensitiveEnum
{
    PlayerMove,
    PlayerRotation,
    ObjectMove,
    ObjectRotation,
    ObjectScale
}

public class SensitiveControl : NeedToDoOnit
{
    public SensitiveEnum thisSensitiveEnum;

    public TextMeshProUGUI SensitiveValue;
    public List<Image> ShowValues;

    public Sprite OnSprite;
    public Sprite OffSprite;

    InputManager inputManager;
    MainCamera mainCamera;

    static List<SensitiveControl> sensitiveControls =
        new List<SensitiveControl>();

    static readonly float[] StepValues =
    {
        0.5f,
        0.6f,
        0.7f,
        0.8f,
        0.9f,
        1.0f,
        1.2f,
        1.4f,
        1.6f,
        1.8f,
        2.0f
    };

    private void OnEnable()
    {
        if (!sensitiveControls.Contains(this))
        {
            sensitiveControls.Add(this);
        }
    }

    private void OnDisable()
    {
        sensitiveControls.Remove(this);
    }

    public override void Onit()
    {
        inputManager = GameManager.Instance.InputManager;
        mainCamera = GameManager.Instance.MainCamera;

        Setting();
    }

    void Setting()
    {
        EnsureManagers();

        float value = GetCurrentValue();
        int stepIndex = GetClosestStepIndex(value);

        SensitiveValue.text =
            "x" + StepValues[stepIndex].ToString("0.0");

        int count = Mathf.Clamp(
            stepIndex,
            0,
            ShowValues.Count
        );

        for (int i = 0; i < ShowValues.Count; i++)
        {
            ShowValues[i].sprite =
                i < count
                ? OnSprite
                : OffSprite;
        }
    }

    public void ClickSensitiveChangeBtn(float direction)
    {
        EnsureManagers();

        int currentIndex =
            GetClosestStepIndex(GetCurrentValue());

        int change =
            direction > 0f
            ? 1
            : direction < 0f
                ? -1
                : 0;

        int nextIndex = Mathf.Clamp(
            currentIndex + change,
            0,
            StepValues.Length - 1
        );

        SetCurrentValue(StepValues[nextIndex]);

        RefreshAll();

        GameManager.Instance
            .SaveAndLoadManager
            .PlayerSetting_SaveSensitive();
    }

    void EnsureManagers()
    {
        if (inputManager == null)
        {
            inputManager = GameManager.Instance.InputManager;
        }

        if (mainCamera == null)
        {
            mainCamera = GameManager.Instance.MainCamera;
        }
    }

    float GetCurrentValue()
    {
        switch (thisSensitiveEnum)
        {
            case SensitiveEnum.PlayerMove:
                return inputManager.PlayerMoveSensitivity;

            case SensitiveEnum.PlayerRotation:
                return mainCamera.PlayerRotationSensitivity;

            case SensitiveEnum.ObjectMove:
                return inputManager.ObjectMoveSensitivity;

            case SensitiveEnum.ObjectRotation:
                return inputManager.ObjectRotationSensitivity;

            case SensitiveEnum.ObjectScale:
                return inputManager.ObjectScaleSensitivity;
        }

        return 1f;
    }

    void SetCurrentValue(float value)
    {
        switch (thisSensitiveEnum)
        {
            case SensitiveEnum.PlayerMove:
                inputManager.SetPlayerMoveSensitivity(value);
                break;

            case SensitiveEnum.PlayerRotation:
                mainCamera.SetPlayerRotationSensitivity(value);
                break;

            case SensitiveEnum.ObjectMove:
                inputManager.SetObjectMoveSensitivity(value);
                break;

            case SensitiveEnum.ObjectRotation:
                inputManager.SetObjectRotationSensitivity(value);
                break;

            case SensitiveEnum.ObjectScale:
                inputManager.SetObjectScaleSensitivity(value);
                break;
        }
    }

    int GetClosestStepIndex(float value)
    {
        int closestIndex = 0;
        float closestDistance =
            Mathf.Abs(value - StepValues[0]);

        for (int i = 1; i < StepValues.Length; i++)
        {
            float distance =
                Mathf.Abs(value - StepValues[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    static void RefreshAll()
    {
        for (int i = 0; i < sensitiveControls.Count; i++)
        {
            if (sensitiveControls[i] != null)
            {
                sensitiveControls[i].Setting();
            }
        }
    }
}
