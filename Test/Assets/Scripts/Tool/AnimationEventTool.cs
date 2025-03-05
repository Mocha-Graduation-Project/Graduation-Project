using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Animations;

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

    [MenuItem("Tools/Animation Event Tool")]
    public static void ShowWindow()
    {
        GetWindow<AnimationEventTool>("Animation Event Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Event Manager", EditorStyles.boldLabel);
        selectedClip = (AnimationClip)EditorGUILayout.ObjectField("アニメーション", selectedClip, typeof(AnimationClip), false);

        if (selectedClip == null) return;
        
        float newTime = EditorGUILayout.Slider("イベントを起こす時間", currentTime, 0, selectedClip.length);
        if (newTime != currentTime)
        {
            currentTime = newTime;
            UpdatePreviewTime();
        }
        
        eventName = EditorGUILayout.TextField("イベントで呼ぶ関数名", eventName);
        
        if (GUILayout.Button("イベント追加"))
        {
            AddAnimationEvent(selectedClip, currentTime, eventName);
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

        previewObject = (GameObject)EditorGUILayout.ObjectField("Preview Object", previewObject, typeof(GameObject), true);
        
        if (selectedClip != null && previewObject != null)
        {
            if (GUILayout.Button("Play Preview"))
            {
                PlayPreview();
            }
        }
    }
    
    private void AddAnimationEvent(AnimationClip clip, float time, string functionName)
    {
        if (string.IsNullOrEmpty(functionName))
        {
            Debug.LogWarning("Event Name cannot be empty.");
            return;
        }

        AnimationEvent animEvent = new AnimationEvent
        {
            time = time,
            functionName = functionName
        };
        
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        System.Array.Resize(ref events, events.Length + 1);
        events[events.Length - 1] = animEvent;
        
        AnimationUtility.SetAnimationEvents(clip, events);
    }
    
    private void ClearAnimationEvents(AnimationClip clip)
    {
        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
    }
    
    private void DisplayExistingEvents()
    {
        if (selectedClip == null) return;
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(selectedClip);

        for (int i = 0; i < events.Length; i++)
        {
            GUILayout.BeginHorizontal();
            events[i].time = EditorGUILayout.FloatField(events[i].time);
            events[i].functionName = EditorGUILayout.TextField(events[i].functionName);
            if (GUILayout.Button("Remove"))
            {
                RemoveAnimationEvent(selectedClip, events[i]);
            }
            GUILayout.EndHorizontal();
        }

        AnimationUtility.SetAnimationEvents(selectedClip, events);
    }
    
    private void RemoveAnimationEvent(AnimationClip clip, AnimationEvent eventToRemove)
    {
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        events = System.Array.FindAll(events, e => e.functionName != eventToRemove.functionName || e.time != eventToRemove.time);
        AnimationUtility.SetAnimationEvents(clip, events);
    }

    private void PlayPreview()
    {
        if (previewObject == null || selectedClip == null) return;
        
        // 既存のPlayableGraphを破棄
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
        
        // PlayableGraphを作成
        playableGraph = PlayableGraph.Create("AnimationPreview");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        
        playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", animator);
        clipPlayable = AnimationClipPlayable.Create(playableGraph, selectedClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        
        playableGraph.Play();
        clipPlayable.SetTime(currentTime); // 再生時間をスライダーに同期
        playableGraph.Stop();
    }

    private void UpdatePreviewTime()
    {
        if (clipPlayable.IsValid())
        {
            clipPlayable.SetTime(currentTime);
            playableGraph.Evaluate(); // グラフを即座に更新
        }
    }
}
