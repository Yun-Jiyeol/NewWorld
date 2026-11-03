using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyEx : MonoBehaviour
{
    [Header("Fixed")]
    public TextMeshProUGUI ExplainText;

    [Header("Templates In This KeyEx")]
    [Tooltip("키보드 키 모양 원본")]
    public GameObject KeyTemplate;

    [Tooltip("마우스 / ~ 같은 Sprite 표시용 원본")]
    public Image SpriteTemplate;

    [Tooltip("설명 뒤에 붙는 ':' 이미지")]
    public RectTransform Separator;

    private readonly List<GameObject> keyPool =
        new List<GameObject>();

    private readonly List<Image> spritePool =
        new List<Image>();

    private readonly List<VisualToken> visualTokens =
        new List<VisualToken>();

    private readonly List<ResolvedInput> resolvedInputs =
        new List<ResolvedInput>();

    // 화면에 보여야 하는 논리 순서대로 저장:
    // Key1, ~, Key6 ...
    private readonly List<Transform> activeVisuals =
        new List<Transform>();

    private int useKeyCount = 0;
    private int useSpriteCount = 0;

    private struct VisualToken
    {
        public bool IsSprite;
        public bool IsShift;
        public string Text;
        public Sprite Sprite;
        public bool OverrideSpriteSize;
        public float Width;
        public float Height;
    }

    private struct ResolvedInput
    {
        public bool Valid;
        public bool NeedShift;
        public bool UseSprite;
        public string Text;
        public Sprite Sprite;
        public bool OverrideSpriteSize;
        public float Width;
        public float Height;
    }

    private void Awake()
    {
        // 이 UI는 표시 전용이므로
        // Image / TextMeshProUGUI가 Raycast를 막지 않게 한다.
        DisableRaycastTargets(
            gameObject
        );

        if (KeyTemplate != null)
            KeyTemplate.SetActive(false);

        if (SpriteTemplate != null)
            SpriteTemplate.gameObject.SetActive(false);
    }

    public void Setting(
        CanControlRowData data,
        string translatedText,
        CanControl canControl)
    {
        ResetKey();

        if (data == null ||
            canControl == null)
            return;

        if (ExplainText != null)
            ExplainText.text = translatedText;

        KeyManager keyManager =
            GameManager.Instance != null
            ? GameManager.Instance.KeyManager
            : null;

        if (keyManager == null)
            return;

        visualTokens.Clear();

        if (data.InputType ==
            CanControlInputType.Keys)
        {
            BuildKeys(
                data,
                keyManager,
                canControl
            );
        }
        else
        {
            BuildRange(
                data,
                keyManager,
                canControl
            );
        }

        // 토큰은 실제로 보일 순서 그대로 생성한다.
        for (int i = 0;
             i < visualTokens.Count;
             i++)
        {
            VisualToken token =
                visualTokens[i];

            if (token.IsSprite)
            {
                AddSprite(
                    token.Sprite,
                    token.OverrideSpriteSize,
                    token.Width,
                    token.Height,
                    token.IsShift,
                    canControl
                );
            }
            else
            {
                AddKey(
                    token.Text,
                    token.IsShift,
                    canControl
                );
            }
        }

        // 최종 화면 순서를 강제로 고정:
        // 설명 -> : -> 입력1 -> ~ -> 입력2 ...
        ApplyVisualOrder();
    }

    private void BuildKeys(
        CanControlRowData data,
        KeyManager keyManager,
        CanControl canControl)
    {
        if (data.Keys == null ||
            data.Keys.Count == 0)
            return;

        resolvedInputs.Clear();

        for (int i = 0;
             i < data.Keys.Count;
             i++)
        {
            ResolvedInput input =
                ResolveInput(
                    data.Keys[i],
                    keyManager,
                    canControl
                );

            if (input.Valid)
                resolvedInputs.Add(input);
        }

        if (resolvedInputs.Count == 0)
            return;

        // Shift는 별도 버튼으로 만들지 않는다.
        // 각 실제 입력의 NeedShift를 그대로 색상 처리에 사용한다.
        for (int i = 0;
             i < resolvedInputs.Count;
             i++)
        {
            AddResolvedToken(
                resolvedInputs[i]
            );
        }
    }

    private void BuildRange(
        CanControlRowData data,
        KeyManager keyManager,
        CanControl canControl)
    {
        ResolvedInput start =
            ResolveInput(
                data.RangeStart,
                keyManager,
                canControl
            );

        ResolvedInput end =
            ResolveInput(
                data.RangeEnd,
                keyManager,
                canControl
            );

        if (!start.Valid &&
            !end.Valid)
            return;

        if (start.Valid)
        {
            AddResolvedToken(start);
        }

        if (start.Valid &&
            end.Valid &&
            canControl.RangeSprite != null)
        {
            visualTokens.Add(
                new VisualToken
                {
                    IsSprite = true,
                    IsShift =
                        start.NeedShift ||
                        end.NeedShift,
                    Sprite =
                        canControl.RangeSprite,
                    OverrideSpriteSize = true,
                    Width =
                        canControl.RangeSpriteWidth,
                    Height =
                        canControl.RangeSpriteHeight
                }
            );
        }

        if (end.Valid)
        {
            AddResolvedToken(end);
        }
    }

    private ResolvedInput ResolveInput(
        KeyNameSaves keyName,
        KeyManager keyManager,
        CanControl canControl)
    {
        ResolvedInput result =
            new ResolvedInput();

        if (!keyManager.KeyboardDictionary
            .TryGetValue(
                keyName,
                out Keyboards keyboard
            ))
        {
            return result;
        }

        if (keyboard == null)
            return result;

        result.Valid = true;
        result.NeedShift =
            keyboard.shiftClick;

        Sprite inputSprite =
            canControl.GetInputSprite(
                keyboard
            );

        if (inputSprite != null)
        {
            result.UseSprite = true;
            result.Sprite =
                inputSprite;
            result.OverrideSpriteSize = true;
            result.Width =
                canControl.MouseSpriteWidth;
            result.Height =
                canControl.MouseSpriteHeight;

            return result;
        }

        // Shift는 따로 표시하므로
        // 실제 입력 이름만 KeyManager에서 가져온다.
        // MouseWheel도 inputType을 유지해야 표시 가능.
        Keyboards displayKeyboard =
            new Keyboards()
            {
                inputType =
                    keyboard.inputType,
                key = keyboard.key,
                shiftClick = false
            };

        // CanControl에서는 Tab 키만 언어 설정과 관계없이
        // 항상 영어 표기인 "Tab"을 사용한다.
        if (displayKeyboard.key == KeyCode.Tab)
        {
            result.Text = "Tab";
        }
        else
        {
            result.Text =
                keyManager.GetKeyFromKeyCode(
                    displayKeyboard
                );
        }

        return result;
    }

    private void AddResolvedToken(
        ResolvedInput input)
    {
        if (input.UseSprite)
        {
            visualTokens.Add(
                new VisualToken
                {
                    IsSprite = true,
                    IsShift = input.NeedShift,
                    Sprite = input.Sprite,
                    OverrideSpriteSize =
                        input.OverrideSpriteSize,
                    Width = input.Width,
                    Height = input.Height
                }
            );
        }
        else
        {
            visualTokens.Add(
                new VisualToken
                {
                    IsSprite = false,
                    IsShift = input.NeedShift,
                    Text = input.Text
                }
            );
        }
    }

    private void AddKey(
        string value,
        bool isShift,
        CanControl canControl)
    {
        if (string.IsNullOrEmpty(value))
            return;

        GameObject key =
            GetKey();

        if (key == null)
            return;

        DisableRaycastTargets(
            key
        );

        Image keyImage =
            key.GetComponent<Image>();

        TextMeshProUGUI text =
            key.GetComponentInChildren<
                TextMeshProUGUI
            >(true);

        if (text != null)
            text.text = value;

        if (isShift &&
            canControl != null)
        {
            if (keyImage != null)
            {
                keyImage.color =
                    canControl
                        .ShiftKeyImageColor;
            }

            if (text != null)
            {
                text.color =
                    canControl
                        .ShiftKeyTextColor;
            }
        }
        else
        {
            RestoreNormalKeyColors(
                keyImage,
                text
            );
        }

        key.SetActive(true);

        activeVisuals.Add(
            key.transform
        );
    }

    private void AddSprite(
        Sprite sprite,
        bool overrideSize,
        float width,
        float height,
        bool isShift,
        CanControl canControl)
    {
        if (sprite == null)
            return;

        Image image =
            GetSprite();

        if (image == null)
            return;

        image.raycastTarget = false;
        image.sprite = sprite;

        // Shift 입력용 Sprite(특히 Range의 ~)도
        // Shift 키와 같은 어두운 Image 색을 사용한다.
        if (isShift &&
            canControl != null)
        {
            image.color =
                canControl.ShiftKeyImageColor;
        }
        else if (SpriteTemplate != null)
        {
            // Pool 재사용 시 이전 Shift 색이 남지 않도록 복구.
            image.color =
                SpriteTemplate.color;
        }

        RectTransform rect =
            image.rectTransform;

        if (rect != null)
        {
            Vector2 size =
                rect.sizeDelta;

            if (overrideSize)
            {
                if (width > 0f)
                    size.x = width;

                if (height > 0f)
                    size.y = height;
            }
            else
            {
                // 템플릿 기본 크기로 되돌림
                if (SpriteTemplate != null)
                {
                    size =
                        SpriteTemplate.rectTransform
                            .sizeDelta;
                }
            }

            rect.sizeDelta = size;
        }

        image.gameObject.SetActive(true);

        activeVisuals.Add(
            image.transform
        );
    }

    private GameObject GetKey()
    {
        if (useKeyCount >=
            keyPool.Count)
        {
            if (KeyTemplate == null)
                return null;

            GameObject newKey =
                Instantiate(
                    KeyTemplate,
                    transform
                );

            newKey.name =
                "GeneratedKey";

            newKey.SetActive(false);

            keyPool.Add(newKey);
        }

        GameObject result =
            keyPool[useKeyCount];

        useKeyCount++;

        return result;
    }

    private Image GetSprite()
    {
        if (useSpriteCount >=
            spritePool.Count)
        {
            if (SpriteTemplate == null)
                return null;

            Image newImage =
                Instantiate(
                    SpriteTemplate,
                    transform
                );

            newImage.name =
                "GeneratedSprite";

            newImage.gameObject
                .SetActive(false);

            spritePool.Add(
                newImage
            );
        }

        Image result =
            spritePool[useSpriteCount];

        useSpriteCount++;

        return result;
    }

    private void ApplyVisualOrder()
    {
        if (ExplainText == null ||
            Separator == null)
            return;

        HorizontalLayoutGroup layout =
            GetComponent<
                HorizontalLayoutGroup
            >();

        bool reverse =
            layout != null &&
            layout.reverseArrangement;

        if (reverse)
        {
            // Reverse Arrangement일 때 Hierarchy는
            // 화면에서 보일 순서의 반대로 둬야 한다.
            //
            // Hierarchy:
            // 입력N ... ~ 입력1 : 설명
            //
            // 화면:
            // 설명 : 입력1 ~ ... 입력N
            for (int i =
                     activeVisuals.Count - 1;
                 i >= 0;
                 i--)
            {
                if (activeVisuals[i] != null)
                    activeVisuals[i]
                        .SetAsLastSibling();
            }

            Separator.SetAsLastSibling();
            ExplainText.transform
                .SetAsLastSibling();
        }
        else
        {
            // Reverse Arrangement가 꺼져 있어도
            // 같은 화면 순서를 유지한다.
            ExplainText.transform
                .SetAsLastSibling();

            Separator.SetAsLastSibling();

            for (int i = 0;
                 i < activeVisuals.Count;
                 i++)
            {
                if (activeVisuals[i] != null)
                    activeVisuals[i]
                        .SetAsLastSibling();
            }
        }

        LayoutRebuilder
            .ForceRebuildLayoutImmediate(
                (RectTransform)transform
            );
    }

    private void RestoreNormalKeyColors(
        Image keyImage,
        TextMeshProUGUI text)
    {
        if (KeyTemplate == null)
            return;

        Image templateImage =
            KeyTemplate.GetComponent<Image>();

        TextMeshProUGUI templateText =
            KeyTemplate.GetComponentInChildren<
                TextMeshProUGUI
            >(true);

        if (keyImage != null &&
            templateImage != null)
        {
            keyImage.color =
                templateImage.color;
        }

        if (text != null &&
            templateText != null)
        {
            text.color =
                templateText.color;
        }
    }

    private void DisableRaycastTargets(
        GameObject root)
    {
        if (root == null)
            return;

        Graphic[] graphics =
            root.GetComponentsInChildren<
                Graphic
            >(true);

        for (int i = 0;
             i < graphics.Length;
             i++)
        {
            if (graphics[i] != null)
            {
                graphics[i]
                    .raycastTarget = false;
            }
        }
    }

    public void ResetKey()
    {
        if (ExplainText != null)
            ExplainText.text = "";

        for (int i = 0;
             i < keyPool.Count;
             i++)
        {
            if (keyPool[i] != null)
                keyPool[i]
                    .SetActive(false);
        }

        for (int i = 0;
             i < spritePool.Count;
             i++)
        {
            if (spritePool[i] == null)
                continue;

            spritePool[i].sprite = null;

            spritePool[i]
                .gameObject
                .SetActive(false);
        }

        useKeyCount = 0;
        useSpriteCount = 0;
        visualTokens.Clear();
        resolvedInputs.Clear();
        activeVisuals.Clear();
    }
}
