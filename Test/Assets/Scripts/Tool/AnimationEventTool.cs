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
    // 引数の種類を表すEnum
    private enum ParamType
    {
        None,
        Int,
        Float,
        String,
        Object
    }
    
    // 引数なしを示すための隠し文字列（String型パラメータを利用）
    private const string NoneParamFlag = "[AET_NONE_PARAM]"; 

    private AnimationClipPlayable clipPlayable;
    private float currentTime;
    private string eventName = "";
    
    // 選択された引数の種類
    private ParamType selectedParamType = ParamType.None;
    
    // 各引数の値
    private float floatParam;
    private int intParam;
    private Object objectParam;
    private string stringParam = "";
    
    private PlayableGraph playableGraph;
    private AnimationPlayableOutput playableOutput;
    private GameObject previewObject;
    private Vector2 scrollPosition;
    private AnimationClip selectedClip;


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

        GUILayout.Label("引数設定 (一つのみ選択可能)", EditorStyles.boldLabel);

        // 引数の種類を選択するポップアップ
        selectedParamType = (ParamType)EditorGUILayout.EnumPopup("引数の種類", selectedParamType);

        // 選択された種類に応じて対応する引数フィールドを表示
        switch (selectedParamType)
        {
            case ParamType.Int:
                // Intが選択された場合は、0も入力できるようにします
                intParam = EditorGUILayout.IntField("Int 引数", intParam);
                break;
            case ParamType.Float:
                floatParam = EditorGUILayout.FloatField("Float 引数", floatParam);
                break;
            case ParamType.String:
                stringParam = EditorGUILayout.TextField("String 引数", stringParam);
                break;
            case ParamType.Object:
                objectParam = EditorGUILayout.ObjectField("Object 引数", objectParam, typeof(Object), true);
                break;
            case ParamType.None:
                // 引数なし
                break;
        }

        if (GUILayout.Button("イベント追加"))
            AddAnimationEvent(selectedClip, currentTime, eventName, selectedParamType);

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


    private void AddAnimationEvent(AnimationClip clip, float time, string functionName, ParamType paramType)
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

        // 選択された引数の種類に応じてAnimationEventにパラメータを設定
        switch (paramType)
        {
            case ParamType.Int:
                animEvent.intParameter = intParam;
                break;
            case ParamType.Float:
                animEvent.floatParameter = floatParam;
                break;
            case ParamType.String:
                animEvent.stringParameter = stringParam;
                break;
            case ParamType.Object:
                animEvent.objectReferenceParameter = objectParam;
                break;
            case ParamType.None:
                // Noneが選択された場合、Stringパラメータにフラグを設定して区別できるようにする
                animEvent.stringParameter = NoneParamFlag;
                break;
        }

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

            // 関数名と時間を表示
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

            // パラメータ情報を表示（一つのみ表示）
            string paramInfo = GetParamInfo(currentEvent);
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
    
    // イベントから引数情報を取得するヘルパーメソッド
    private string GetParamInfo(AnimationEvent animEvent)
    {
        // 1. None (引数なし) の隠しフラグを最優先でチェック
        if (animEvent.stringParameter == NoneParamFlag)
        {
            return "Params: None";
        }
        
        // 2. Object引数が設定されているかチェック
        if (animEvent.objectReferenceParameter != null)
        {
            return $"Object: {animEvent.objectReferenceParameter.name}";
        }
        
        // 3. String引数が設定されているかチェック（Noneフラグとは異なる文字列）
        if (!string.IsNullOrEmpty(animEvent.stringParameter))
        {
            return $"String: \"{animEvent.stringParameter}\"";
        }
        
        // 4. IntまたはFloatのチェック
        // Object, Stringが設定されていない場合、intParameterとfloatParameterのどちらが使われたか判断する

        // floatParameterが0以外、かつintParameterが0のときにFloatと見なす（Floatが使われた可能性が高い）
        if (Mathf.Abs(animEvent.floatParameter) > 0.0001f && animEvent.intParameter == 0)
        {
             return $"Float: {animEvent.floatParameter:F3}";
        }
        
        // intParameterが0以外の場合、またはfloatParameterも0.0fの場合（Int: 0の可能性がある）
        // Int: 0 または Int: [非0] の表示を優先します。
        // intParameterはデフォルトで0であり、floatParameterも0.0fのとき、Noneフラグがなければ
        // 実際には意図的にInt: 0が選ばれたか、引数が何も設定されなかった（旧イベント）のいずれかですが、
        // Noneフラグを導入したため、ここでは Int を選択した場合のみ intParameter が利用されていると見なします。
        if (animEvent.intParameter != 0 || Mathf.Abs(animEvent.floatParameter) < 0.0001f) // Int: 0 の場合を含む
        {
            // Int: 0 のイベントが正しく表示されるようにします
            return $"Int: {animEvent.intParameter}";
        }
        
        // 5. それでも特定できない場合はFloat（例えばFloat: 0.0f）
        if (Mathf.Abs(animEvent.floatParameter) > 0.0001f)
        {
            return $"Float: {animEvent.floatParameter:F3}";
        }
        
        // 理論上はここには来ないはずですが、念のため
        return "Params: None (Fallback)";
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

        playableGraph.Evaluate(); // 停止状態で時間を更新するために必要
    }
    
    // ウィンドウが閉じられたときにPlayableGraphを破棄
    private void OnDestroy()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
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