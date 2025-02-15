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
        EditorGUI.PropertyField(position, property, label);
    }
}

#endif