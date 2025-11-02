#region

using System.Collections.Generic;
using Systems;
using UnityEditor;

#endregion

[CustomEditor(typeof(EnemySpawnManager))]
public class EnemySpawnManagerEditor : Editor
{
    private List<bool> conditionFoldouts = new();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("warningMarkerPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("warningTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameClearDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("soundData"), true);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("deathDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("knockDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundLayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemiesToSpawn"), true);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionToSpawn"), true);
        EditorGUILayout.Space();


        serializedObject.ApplyModifiedProperties();
    }
}