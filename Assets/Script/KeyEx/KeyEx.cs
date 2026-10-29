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

    private int useKeyCount = 0;
    private int useSpriteCount = 0;

    private struct VisualToken
    {
        public bool IsSprite;
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

        // HorizontalLayoutGroup Reverse Arrangement 기준.
        for (int i = visualTokens.Count - 1;
             i >= 0;
             i--)
        {
            VisualToken token =
                visualTokens[i];

            if (token.IsSprite)
                AddSprite(
                    token.Sprite,
                    token.OverrideSpriteSize,
                    token.Width,
                    token.Height
                );
            else
                AddKey(token.Text);
        }
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

        bool sameShift = true;
        bool firstShift =
            resolvedInputs[0].NeedShift;

        for (int i = 1;
             i < resolvedInputs.Count;
             i++)
        {
            if (resolvedInputs[i].NeedShift !=
                firstShift)
            {
                sameShift = false;
                break;
            }
        }

        // 전부 Shift 필요면 Shift를 한 번만.
        if (sameShift &&
            firstShift)
        {
            AddShiftToken(
                keyManager
            );
        }

        for (int i = 0;
             i < resolvedInputs.Count;
             i++)
        {
            if (!sameShift &&
                resolvedInputs[i].NeedShift)
            {
                AddShiftToken(
                    keyManager
                );
            }

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

        bool sameShift =
            start.Valid &&
            end.Valid &&
            start.NeedShift ==
            end.NeedShift;

        if (sameShift &&
            start.NeedShift)
        {
            AddShiftToken(
                keyManager
            );
        }

        if (start.Valid)
        {
            if (!sameShift &&
                start.NeedShift)
            {
                AddShiftToken(
                    keyManager
                );
            }

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
            if (!sameShift &&
                end.NeedShift)
            {
                AddShiftToken(
                    keyManager
                );
            }

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

        result.Text =
            keyManager.GetKeyFromKeyCode(
                displayKeyboard
            );

        return result;
    }

    private void AddShiftToken(
        KeyManager keyManager)
    {
        Keyboards shiftKeyboard =
            new Keyboards()
            {
                key = KeyCode.LeftShift,
                shiftClick = false
            };

        visualTokens.Add(
            new VisualToken
            {
                IsSprite = false,
                Text =
                    keyManager
                        .GetKeyFromKeyCode(
                            shiftKeyboard
                        )
            }
        );
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
                    Text = input.Text
                }
            );
        }
    }

    private void AddKey(
        string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        GameObject key =
            GetKey();

        if (key == null)
            return;

        TextMeshProUGUI text =
            key.GetComponentInChildren<
                TextMeshProUGUI
            >(true);

        if (text != null)
            text.text = value;

        key.SetActive(true);
    }

    private void AddSprite(
        Sprite sprite,
        bool overrideSize,
        float width,
        float height)
    {
        if (sprite == null)
            return;

        Image image =
            GetSprite();

        if (image == null)
            return;

        image.sprite = sprite;

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

            MoveBeforeSeparator(
                newKey.transform
            );

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

            MoveBeforeSeparator(
                newImage.transform
            );

            spritePool.Add(
                newImage
            );
        }

        Image result =
            spritePool[useSpriteCount];

        useSpriteCount++;

        return result;
    }

    private void MoveBeforeSeparator(
        Transform target)
    {
        if (Separator == null)
            return;

        // HorizontalLayoutGroup의 Reverse Arrangement 때문에
        // Hierarchy에서는 입력을 ':' 보다 앞에 둔다.
        //
        // Hierarchy:
        // GeneratedInput -> : -> Explaintext
        //
        // 실제 화면:
        // Explaintext -> : -> GeneratedInput
        target.SetSiblingIndex(
            Separator.GetSiblingIndex()
        );
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
    }
}
