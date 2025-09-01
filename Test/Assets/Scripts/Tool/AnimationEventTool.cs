using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;

public class AnimationEventTool : EditorWindow
{
    private AnimationClip selectedClip;
    private float currentTime;
    private string eventName = "";
    private Vector2 scrollPosition;
    private GameObject previewObject;
    private PlayableGraph playableGraph;
    private AnimationPlayableOutput playableOutput;
    private AnimationClipPlayable clipPlayable;
    private int intParam;
    private float floatParam;
    private string stringParam = "";
    private Object objectParam;
    private bool useIntParam, useFloatParam, useStringParam, useObjectParam;
    
    [MenuItem("Tools/Animation Event Tool")]
    public static void ShowWindow()
    {
        GetWindow<AnimationEventTool>("Animation Event Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Event Manager", EditorStyles.boldLabel);
        selectedClip =
            (AnimationClip)EditorGUILayout.ObjectField("アニメーション", selectedClip, typeof(AnimationClip), false);
        if (selectedClip == null) return;

        float newTime = EditorGUILayout.Slider("イベントを起こす時間", currentTime, 0, selectedClip.length);
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
        {
            AddAnimationEvent(selectedClip, currentTime, eventName, useIntParam ? intParam : 0,
                useFloatParam ? floatParam : 0f, useStringParam ? stringParam : "",
                useObjectParam ? objectParam : null);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("イベントリセット"))
        {
            ClearAnimationEvents(selectedClip);
        }

        GUILayout.Space(10);
        GUILayout.Label("追加したイベント:", EditorStyles.boldLabel);
        DisplayExistingEvents();

        GUILayout.Space(20);
        GUILayout.Label("プレビュー", EditorStyles.boldLabel);
        previewObject =
            (GameObject)EditorGUILayout.ObjectField("Preview Object", previewObject, typeof(GameObject), true);

        if (selectedClip != null && previewObject != null)
        {
            if (GUILayout.Button("Play Preview"))
            {
                PlayPreview();
            }
        }
    }

    private void AddAnimationEvent(AnimationClip clip, float time, string functionName, int intParam, float floatParam, string stringParam, Object objectParam)
    {
        if (string.IsNullOrEmpty(functionName))
        {
            Debug.LogWarning("Event Name cannot be empty.");
            return;
        }

        // 変更を記録
        Undo.RecordObject(clip, "Add Animation Event");

        AnimationEvent animEvent = new AnimationEvent
        {
            time = time,
            functionName = functionName
        };

        // 使用する引数のみ設定
        if (useIntParam) animEvent.intParameter = intParam;
        if (useFloatParam) animEvent.floatParameter = floatParam;
        if (useStringParam) animEvent.stringParameter = stringParam;
        if (useObjectParam) animEvent.objectReferenceParameter = objectParam;
        
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        System.Array.Resize(ref events, events.Length + 1);
        events[events.Length - 1] = animEvent;

        AnimationUtility.SetAnimationEvents(clip, events);

        // 変更を保存
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
    }

    
    private void ClearAnimationEvents(AnimationClip clip)
    {
        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
    }
    
    private void DisplayExistingEvents()
    {
        if (selectedClip == null) return;
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(selectedClip);
        int removeIndex = -1; // 削除対象のインデックスを保持

        for (int i = 0; i < events.Length; i++)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(events[i].functionName, GUILayout.Width(150));
            EditorGUILayout.LabelField(events[i].time.ToString("F2"), GUILayout.Width(50));

            string paramInfo = $"Int: {events[i].intParameter}, Float: {events[i].floatParameter}, String: {events[i].stringParameter}, Object: {events[i].objectReferenceParameter}";
            EditorGUILayout.LabelField(paramInfo, GUILayout.Width(300));

            if (GUILayout.Button("Remove"))
            {
                removeIndex = i; // 削除対象を記録
            }
            GUILayout.EndHorizontal();
        }

        // ループ終了後に削除処理を実行
        if (removeIndex >= 0)
        {
            RemoveAnimationEvent(selectedClip, removeIndex);
        }
    }


    private void RemoveAnimationEvent(AnimationClip clip, int index)
    {
        List<AnimationEvent> eventList = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip));
        if (index < 0 || index >= eventList.Count) return;

        eventList.RemoveAt(index);
        AnimationUtility.SetAnimationEvents(clip, eventList.ToArray());
    }

    
    private void PlayPreview()
    {
        if (previewObject == null || selectedClip == null) return;
        
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
        
        Animator animator = previewObject.GetComponent<Animator>();
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