using UnityEditor;
using UnityEngine;
using Systems;
using System.Collections.Generic;

[CustomEditor(typeof(EnemySpawnManager))]
public class EnemySpawnManagerEditor : Editor
{
    private List<bool> conditionFoldouts = new List<bool>();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("warningMarkerPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("warningTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameClearDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("soundData"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemiesToSpawn"), true);

        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionToSpawn"), true);
        EditorGUILayout.Space();


        serializedObject.ApplyModifiedProperties();
    }

}
