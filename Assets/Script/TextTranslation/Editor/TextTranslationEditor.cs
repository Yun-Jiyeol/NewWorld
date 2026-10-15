using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextTranslation))]
[CanEditMultipleObjects]
public class TextTranslationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField(
            "Translation Preview",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "Selected : " + targets.Length
        );

        // =========================
        // 1줄
        // =========================

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("COM"))
        {
            ApplyPreview(LanguageEnum.Com);
        }

        if (GUILayout.Button("KOR"))
        {
            ApplyPreview(LanguageEnum.Kor);
        }

        if (GUILayout.Button("ENG"))
        {
            ApplyPreview(LanguageEnum.Eng);
        }

        EditorGUILayout.EndHorizontal();


        // =========================
        // 2줄
        // =========================

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("CHS"))
        {
            ApplyPreview(LanguageEnum.Chs);
        }

        if (GUILayout.Button("CHT"))
        {
            ApplyPreview(LanguageEnum.Cht);
        }

        if (GUILayout.Button("JPN"))
        {
            ApplyPreview(LanguageEnum.Jpn);
        }

        EditorGUILayout.EndHorizontal();
    }


    void ApplyPreview(LanguageEnum language)
    {
        // 선택된 TextTranslation 전부 순회
        foreach (Object obj in targets)
        {
            TextTranslation translation =
                obj as TextTranslation;

            if (translation == null)
                continue;


            TextMeshProUGUI text =
                translation.GetComponent<TextMeshProUGUI>();

            if (text == null)
            {
                Debug.LogWarning(
                    "TextMeshProUGUI가 없습니다.",
                    translation
                );

                continue;
            }


            // Ctrl + Z 지원
            Undo.RecordObject(
                text,
                "Translation Preview"
            );


            translation.PreviewSetting(language);


            // 변경사항 저장 대상으로 표시
            EditorUtility.SetDirty(text);
        }


        SceneView.RepaintAll();
    }
}