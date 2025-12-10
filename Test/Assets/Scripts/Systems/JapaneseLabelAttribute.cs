using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class JapaneseLabelAttribute : PropertyAttribute
{
    public string label;

    public JapaneseLabelAttribute(string label)
    {
        this.label = label;
    }
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(JapaneseLabelAttribute))]
public class JapaneseLabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        JapaneseLabelAttribute japaneseLabel = (JapaneseLabelAttribute)attribute;
        label.text = japaneseLabel.label;
        //EditorGUI.PropertyField(position, property, label);
        // 安全チェック：オブジェクト参照かつ、その中身が「保存禁止」フラグを持っているか確認
        if (IsUnsafeRuntimeObject(property))
        {
            // PropertyFieldを使わずに、単なるラベルとして表示（エラー回避）
            EditorGUI.LabelField(position, label, new GUIContent("(Runtime Object - Not Editable)"));
        }
        else
        {
            // 通常通り描画
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
    private bool IsUnsafeRuntimeObject(SerializedProperty property)
    {
        // プロパティがオブジェクト参照以外なら安全
        if (property.propertyType != SerializedPropertyType.ObjectReference) return false;
        
        // 中身がnullなら安全
        if (property.objectReferenceValue == null) return false;

        // DontSaveInEditor フラグが立っているかチェック
        return (property.objectReferenceValue.hideFlags & HideFlags.DontSaveInEditor) != 0;
    }
}

#endif