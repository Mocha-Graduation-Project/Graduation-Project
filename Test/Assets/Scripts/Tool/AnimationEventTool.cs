#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

// プレビューオブジェクトにアタッチしてイベントを受け取るためのヘルパーコンポーネント
public class AnimationEventToolHelper : MonoBehaviour
{
    public const string NoneParamFlag = "[AET_NONE_PARAM]"; 

    private AnimationEvent[] events;
    private AnimationClip clip;
    private float lastTime = -1f;

    private Dictionary<string, (ParticleSystem ps, float startTime)> activeVfx = new Dictionary<string, (ParticleSystem ps, float startTime)>();
    
    // ツールからの初期化時に呼ばれる
    public void Setup(AnimationClip targetClip)
    {
        clip = targetClip;
        events = AnimationUtility.GetAnimationEvents(clip);
        lastTime = -1f;
        
        StopAllVfx();
    }
    
    // 外部から呼ばれるイベント処理関数
    public void PlayVfx(string vfxName, float eventTime)
    {
        if (vfxName == NoneParamFlag) return;

        if (activeVfx.ContainsKey(vfxName))
        {
            activeVfx[vfxName].ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            activeVfx.Remove(vfxName);
        }

        Transform vfxChild = transform.Find(vfxName);
        if (vfxChild != null)
        {
            var particleSystem = vfxChild.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // Looping設定の警告
                if (particleSystem.main.loop)
                {
                    Debug.LogWarning($"<color=red>[AET Helper]</color> VFX '{vfxName}' is looping. It will not stop automatically based on Duration. Please disable looping.");
                }
                
                // VFXを再生し、アクティブリストにイベント発生時刻を記録
                particleSystem.Play(true);
                activeVfx.Add(vfxName, (particleSystem, eventTime));
            }
            else
            {
                Debug.LogWarning($"<color=red>ParticleSystem component not found</color> on child object: {vfxChild.name}.");
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>VFX GameObject not found</color> with name: '{vfxName}' as a child of the Preview Object.");
        }
    }
    
    // スライダー操作時にツールから呼ばれる (VFXの再生・停止ロジックを制御)
    public void UpdateVfxState(float currentTime)
    {
        if (clip == null || events == null || lastTime < 0)
        {
            lastTime = currentTime;
            return;
        }

        // 時間が大きくジャンプした（逆再生、またはスライダーの急な移動）場合は、全VFXを停止
        if (Mathf.Abs(currentTime - lastTime) > 0.5f || currentTime < lastTime)
        {
            StopAllVfx();
            activeVfx.Clear(); // activeVfxもクリア
            lastTime = currentTime;
            return;
        }

        var toRemove = new List<string>();
        foreach (var pair in activeVfx)
        {
            var vfxName = pair.Key;
            var ps = pair.Value.ps;
            var startTime = pair.Value.startTime;
            
            // Loopingでない場合のみ、Durationに基づき終了判定を行う
            if (!ps.main.loop)
            {
                float duration = ps.main.duration;
                // 再生開始時刻からDurationを超えているかチェック
                if (currentTime >= startTime + duration)
                {
                    // 終了したParticleSystemを明示的に停止
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    toRemove.Add(vfxName);
                }
            }
            else
            {
                // Loopingの場合でも、isStoppedがtrueになったら除去
                if (ps.isStopped) 
                {
                     toRemove.Add(vfxName);
                }
            }
        }

        foreach (var key in toRemove)
        {
            activeVfx.Remove(key);
        }

        foreach (var animEvent in events)
        {
            float eventTime = animEvent.time;
            
            // イベントが今回の時間ステップで発生したかどうかをチェック
            // (lastTime, currentTime] の範囲でチェック
            if (eventTime > lastTime && eventTime <= currentTime)
            {
                if (animEvent.functionName == "PlayVfx")
                {
                    // イベント発生時刻を渡す
                    PlayVfx(animEvent.stringParameter, eventTime);
                }
            }
        }

        lastTime = currentTime;
    }

    // すべてのParticleSystemを停止し、リストをクリアする
    public void StopAllVfx()
    {
        // アクティブリストにあるVFXを停止
        foreach (var pair in activeVfx)
        {
            pair.Value.ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        activeVfx.Clear(); // 実行中のVFXリストもクリア

        // 念のため、子階層の全てのParticleSystemも停止
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            if (ps.isPlaying)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
    
    public void Cleanup()
    {
        StopAllVfx();
    }
}


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
    
    private ParamType selectedParamType = ParamType.None;
    
    private float floatParam;
    private int intParam;
    private Object objectParam;
    private string stringParam = "";
    
    private PlayableGraph playableGraph;
    private AnimationPlayableOutput playableOutput;
    private GameObject previewObject;
    private AnimationEventToolHelper helper;
    private Vector2 scrollPosition;
    private AnimationClip selectedClip;


    private void OnGUI()
    {
        GUILayout.Label("Animation Event Manager", EditorStyles.boldLabel);

        selectedClip =
            (AnimationClip)EditorGUILayout.ObjectField("アニメーション", selectedClip, typeof(AnimationClip), false);

        if (selectedClip == null) return;


        var newTime = EditorGUILayout.Slider("プレビュー時間", currentTime, 0, selectedClip.length);

        // スライダーが動いたときにVFXの再生状態を更新
        if (Mathf.Abs(newTime - currentTime) > 0.001f)
        {
            currentTime = newTime;
            UpdatePreviewTime();
        }


        eventName = EditorGUILayout.TextField("イベントで呼ぶ関数名", eventName);


        GUILayout.Space(5);

        GUILayout.Label("引数設定 (一つのみ選択可能)", EditorStyles.boldLabel);

        selectedParamType = (ParamType)EditorGUILayout.EnumPopup("引数の種類", selectedParamType);

        switch (selectedParamType)
        {
            case ParamType.Int:
                intParam = EditorGUILayout.IntField("Int 引数", intParam);
                break;
            case ParamType.Float:
                floatParam = EditorGUILayout.FloatField("Float 引数", floatParam);
                break;
            case ParamType.String:
                stringParam = EditorGUILayout.TextField("String 引数 (VFX名に推奨)", stringParam);
                break;
            case ParamType.Object:
                objectParam = EditorGUILayout.ObjectField("Object 引数", objectParam, typeof(Object), true);
                break;
            case ParamType.None:
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
            (GameObject)EditorGUILayout.ObjectField("Preview Object (Animator必須)", previewObject, typeof(GameObject), true);


        if (selectedClip != null && previewObject != null)
        {
            // ボタンを「Setup Preview」に統一
            if (GUILayout.Button("Setup Preview (アニメーションとVFX連動開始)"))
                SetupPreview();
        }
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
                animEvent.stringParameter = NoneParamFlag;
                break;
        }

        var events = AnimationUtility.GetAnimationEvents(clip);

        Array.Resize(ref events, events.Length + 1);

        events[events.Length - 1] = animEvent;

        AnimationUtility.SetAnimationEvents(clip, events);

        EditorUtility.SetDirty(clip);

        AssetDatabase.SaveAssets();
        
        // イベント追加後、ヘルパーを再セットアップ
        if (helper != null)
        {
            helper.Setup(clip);
        }
    }


    private void ClearAnimationEvents(AnimationClip clip)
    {
        Undo.RecordObject(clip, "Clear Animation Events");

        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);

        EditorUtility.SetDirty(clip);

        AssetDatabase.SaveAssets();
        
        // イベントクリア後、ヘルパーを再セットアップ
        if (helper != null)
        {
            helper.Setup(clip);
        }
    }


    private void DisplayExistingEvents()
    {
        if (selectedClip == null) return;

        var events = AnimationUtility.GetAnimationEvents(selectedClip);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (var i = 0; i < events.Length; i++)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            var currentEvent = events[i];

            EditorGUILayout.LabelField($"関数名: {currentEvent.functionName}", GUILayout.Width(150));
            
            var newTime = EditorGUILayout.Slider(currentEvent.time, 0, selectedClip.length);

            if (Mathf.Abs(newTime - currentEvent.time) > 0.001f)
            {
                Undo.RecordObject(selectedClip, "Change Animation Event Time");

                currentEvent.time = newTime;

                AnimationUtility.SetAnimationEvents(selectedClip, events);

                EditorUtility.SetDirty(selectedClip);
                
                if (clipPlayable.IsValid())
                {
                    UpdatePreviewTime();
                }
                
                // イベント変更後、ヘルパーを再セットアップ
                if (helper != null)
                {
                    helper.Setup(selectedClip);
                }
            }

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
    
    private string GetParamInfo(AnimationEvent animEvent)
    {
        if (animEvent.stringParameter == NoneParamFlag)
        {
            return "Params: None";
        }
        
        if (animEvent.objectReferenceParameter != null)
        {
            return $"Object: {animEvent.objectReferenceParameter.name}";
        }
        
        if (!string.IsNullOrEmpty(animEvent.stringParameter))
        {
            return $"String: \"{animEvent.stringParameter}\"";
        }
        
        if (Mathf.Abs(animEvent.floatParameter) > 0.0001f && animEvent.intParameter == 0)
        {
             return $"Float: {animEvent.floatParameter:F3}";
        }
        
        if (animEvent.intParameter != 0 || Mathf.Abs(animEvent.floatParameter) < 0.0001f)
        {
            return $"Int: {animEvent.intParameter}";
        }
        
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
        
        // イベント削除後、ヘルパーを再セットアップ
        if (helper != null)
        {
            helper.Setup(clip);
        }
    }


    private void SetupPreview()
    {
        if (previewObject == null || selectedClip == null) return;


        if (playableGraph.IsValid()) playableGraph.Destroy();

        // 既存のヘルパーがあればクリーンアップ
        helper = previewObject.GetComponent<AnimationEventToolHelper>();
        if (helper != null)
        {
            helper.Cleanup();
        }


        var animator = previewObject.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("Preview Object needs an Animator component.");
            return;
        }

        // Helper Componentの追加と初期化
        helper = previewObject.GetComponent<AnimationEventToolHelper>();
        if (helper == null)
        {
            helper = previewObject.AddComponent<AnimationEventToolHelper>();
        }
        // Helperにクリップ情報を渡し、VFXを停止
        helper.Setup(selectedClip);


        playableGraph = PlayableGraph.Create("AnimationPreview");

        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);


        playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", animator);

        clipPlayable = AnimationClipPlayable.Create(playableGraph, selectedClip);

        playableOutput.SetSourcePlayable(clipPlayable);


        // 初期時間に設定
        clipPlayable.SetTime(currentTime);

        playableGraph.Evaluate(); 
        
        // 初期状態のVFXを更新
        helper.UpdateVfxState(currentTime);
    }
    

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
        
        if (helper != null)
        {
            helper.Cleanup();
        }
    }


    private void UpdatePreviewTime()
    {
        if (clipPlayable.IsValid())
        {
            // PlayableGraphの評価（アニメーションのポーズ更新）
            clipPlayable.SetTime(currentTime);
            playableGraph.Evaluate();
            
            // Helperに時間の変化を通知し、VFXの状態を更新
            if (helper != null)
            {
                helper.UpdateVfxState(currentTime);
            }
        }
    }
}

#endif