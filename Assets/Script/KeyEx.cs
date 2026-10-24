using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyEx : MonoBehaviour
{
    public Image KeyImage;
    public TextMeshProUGUI SettingKey;
    public TextMeshProUGUI ExplainText;

    [Header("Color")]
    public Color NormalColor = Color.white;

    // Shift가 필요한 키는 흰색의 약 1/10 밝기로 표시
    public Color ShiftColor =
        new Color(0.1f, 0.1f, 0.1f, 1f);

    public void Setting(
        string settingKey,
        string explainText,
        bool needShift)
    {
        if (SettingKey != null)
        {
            SettingKey.text = settingKey;
        }

        if (ExplainText != null)
        {
            ExplainText.text = explainText;
        }

        Color keyColor =
            needShift ? ShiftColor : NormalColor;

        if (KeyImage != null)
        {
            KeyImage.color = keyColor;
        }

        if (SettingKey != null)
        {
            SettingKey.color = keyColor;
        }
    }

    public void ResetKey()
    {
        if (SettingKey != null)
        {
            SettingKey.text = "";
            SettingKey.color = NormalColor;
        }

        if (ExplainText != null)
        {
            ExplainText.text = "";
        }

        if (KeyImage != null)
        {
            KeyImage.color = NormalColor;
        }
    }
}
