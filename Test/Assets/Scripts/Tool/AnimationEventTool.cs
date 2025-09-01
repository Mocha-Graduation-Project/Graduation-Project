#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

public class AnimationEventTool : EditorWindow

{
    private AnimationClipPlayable clipPlayable;

    private float currentTime;

    private string eventName = "";

    private float floatParam;

    private int intParam;

    private Object objectParam;

    private PlayableGraph playableGraph;

    private AnimationPlayableOutput playableOutput;

    private GameObject previewObject;

    private Vector2 scrollPosition;

    private AnimationClip selectedClip;

    private string stringParam = "";

    private bool useIntParam, useFloatParam, useStringParam, useObjectParam;


    private void OnGUI()

    {
        GUILayout.Label("Animation Event Manager", EditorStyles.boldLabel);

        selectedClip =
            (AnimationClip)EditorGUILayout.ObjectField("アニメーション", selectedClip, typeof(AnimationClip), false);

        if (selectedClip == null) return;


        var newTime = EditorGUILayout.Slider("イベントを起こす時間", currentTime, 0, selectedClip.length);

        if (newTime != currentTime)

        {
            currentTime = newTime;

            UpdatePreviewTime();
        }


        eventName = EditorGUILayout.TextField("イベントで呼ぶ関数名", eventName);


        GUILayout.Space(5);

        GUILayout.Label("引数設定", EditorStyles.boldLabel);


        useIntParam = EditorGUILayout.Toggle("Int を使用", useIntParam);

        if (useIntParam) intParam = EditorGUILayout.IntField("Int 引数", intParam);


        useFloatParam = EditorGUILayout.Toggle("Float を使用", useFloatParam);

        if (useFloatParam) floatParam = EditorGUILayout.FloatField("Float 引数", floatParam);


        useStringParam = EditorGUILayout.Toggle("String を使用", useStringParam);

        if (useStringParam) stringParam = EditorGUILayout.TextField("String 引数", stringParam);


        useObjectParam = EditorGUILayout.Toggle("Object を使用", useObjectParam);

        if (useObjectParam) objectParam = EditorGUILayout.ObjectField("Object 引数", objectParam, typeof(Object), true);

        if (GUILayout.Button("イベント追加"))
            AddAnimationEvent(selectedClip, currentTime, eventName, useIntParam ? intParam : 0,
                useFloatParam ? floatParam : 0f, useStringParam ? stringParam : "",
                useObjectParam ? objectParam : null);

        GUILayout.Space(10);

        if (GUILayout.Button("イベントリセット")) ClearAnimationEvents(selectedClip);


        GUILayout.Space(10);

        GUILayout.Label("追加したイベント:", EditorStyles.boldLabel);

        DisplayExistingEvents();


        GUILayout.Space(20);

        GUILayout.Label("プレビュー", EditorStyles.boldLabel);

        previewObject =
            (GameObject)EditorGUILayout.ObjectField("Preview Object", previewObject, typeof(GameObject), true);


        if (selectedClip != null && previewObject != null)

            if (GUILayout.Button("Play Preview"))

                PlayPreview();
    }


    [MenuItem("Tools/Animation Event Tool")]
    public static void ShowWindow()

    {
        GetWindow<AnimationEventTool>("Animation Event Tool");
    }


    private void AddAnimationEvent(AnimationClip clip, float time, string functionName, int intParam, float floatParam,
        string stringParam, Object objectParam)

    {
        if (string.IsNullOrEmpty(functionName))

        {
            Debug.LogWarning("Event Name cannot be empty.");

            return;
        }

        Undo.RecordObject(clip, "Add Animation Event");


        var animEvent = new AnimationEvent

        {
            time = time,

            functionName = functionName
        };

        if (useIntParam) animEvent.intParameter = intParam;

        if (useFloatParam) animEvent.floatParameter = floatParam;

        if (useStringParam) animEvent.stringParameter = stringParam;

        if (useObjectParam) animEvent.objectReferenceParameter = objectParam;

        var events = AnimationUtility.GetAnimationEvents(clip);

        Array.Resize(ref events, events.Length + 1);

        events[events.Length - 1] = animEvent;

        AnimationUtility.SetAnimationEvents(clip, events);

        EditorUtility.SetDirty(clip);

        AssetDatabase.SaveAssets();
    }


    private void ClearAnimationEvents(AnimationClip clip)

    {
        Undo.RecordObject(clip, "Clear Animation Events");

        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);

        EditorUtility.SetDirty(clip);

        AssetDatabase.SaveAssets();
    }


    private void DisplayExistingEvents()

    {
        if (selectedClip == null) return;


        var events = AnimationUtility.GetAnimationEvents(selectedClip);


// スクロール可能なエリアを作成

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);


        for (var i = 0; i < events.Length; i++)

        {
            GUILayout.BeginHorizontal(GUI.skin.box); // 各イベントをボックスで囲む


            var currentEvent = events[i];


// 関数名と引数情報を表示

            EditorGUILayout.LabelField($"関数名: {currentEvent.functionName}", GUILayout.Width(150));


// 時間を編集可能なスライダーで表示

            var newTime = EditorGUILayout.Slider(currentEvent.time, 0, selectedClip.length);

            if (Mathf.Abs(newTime - currentEvent.time) > 0.001f) // 変更があったかチェック

            {
// 変更を記録し、イベントの時間を更新

                Undo.RecordObject(selectedClip, "Change Animation Event Time");

                currentEvent.time = newTime;

                events[i] = currentEvent; // 更新したイベントを配列に再代入

                AnimationUtility.SetAnimationEvents(selectedClip, events);

                EditorUtility.SetDirty(selectedClip);
            }


// パラメータ情報を表示（今回はシンプルに表示）

            var paramInfo = $"Params: Int={currentEvent.intParameter}, Float={currentEvent.floatParameter}";

            EditorGUILayout.LabelField(paramInfo, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Remove", GUILayout.Width(60)))

            {
                RemoveAnimationEvent(selectedClip, i);

                break;
            }


            GUILayout.EndHorizontal();
        }


        EditorGUILayout.EndScrollView();
    }


    private void RemoveAnimationEvent(AnimationClip clip, int index)

    {
        Undo.RecordObject(clip, "Remove Animation Event");

        var eventList = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip));

        if (index < 0 || index >= eventList.Count) return;


        eventList.RemoveAt(index);

        AnimationUtility.SetAnimationEvents(clip, eventList.ToArray());

        EditorUtility.SetDirty(clip);

        AssetDatabase.SaveAssets();
    }


    private void PlayPreview()

    {
        if (previewObject == null || selectedClip == null) return;


        if (playableGraph.IsValid()) playableGraph.Destroy();


        var animator = previewObject.GetComponent<Animator>();

        if (animator == null)

        {
            Debug.LogWarning("Preview Object needs an Animator component.");

            return;
        }


        playableGraph = PlayableGraph.Create("AnimationPreview");

        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);


        playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", animator);

        clipPlayable = AnimationClipPlayable.Create(playableGraph, selectedClip);

        playableOutput.SetSourcePlayable(clipPlayable);


        playableGraph.Play();

        clipPlayable.SetTime(currentTime);

        playableGraph.Stop();
    }


    private void UpdatePreviewTime()

    {
        if (clipPlayable.IsValid())

        {
            clipPlayable.SetTime(currentTime);

            playableGraph.Evaluate();
        }
    }
}

#endif