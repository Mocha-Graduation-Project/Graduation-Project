using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.Events;

public class AnimationEventTool : EditorWindow
{
    private AnimationClip selectedClip;
    private float currentTime;
    private string eventName = "";
    private int selectedTab = 0;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Animation Event Tool")]
    public static void ShowWindow()
    {
        GetWindow<AnimationEventTool>("Animation Event Tool");
    }

    private void OnGUI()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, new string[] {"Event Manager", "Animation Clips"});

        switch (selectedTab)
        {
            case 0:
                DrawEventManagerTab();
                break;
            case 1:
                DrawAnimationClipTab();
                break;
        }
    }

    private void DrawEventManagerTab()
    {
        GUILayout.Label("Animation Event Manager", EditorStyles.boldLabel);
        selectedClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", selectedClip, typeof(AnimationClip), false);

        if (selectedClip == null) return;
        
        currentTime = EditorGUILayout.Slider("Event Time", currentTime, 0, selectedClip.length);
        eventName = EditorGUILayout.TextField("Event Name", eventName);
        
        if (GUILayout.Button("Add Event"))
        {
            AddAnimationEvent(selectedClip, currentTime, eventName);
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("Clear Events"))
        {
            ClearAnimationEvents(selectedClip);
        }

        GUILayout.Space(10);
        GUILayout.Label("Existing Events:", EditorStyles.boldLabel);
        DisplayExistingEvents();
    }
    
    private void DrawAnimationClipTab()
    {
        GUILayout.Label("Animation Clips", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        AnimationClip[] clips = Resources.FindObjectsOfTypeAll<AnimationClip>();
        
        foreach (var clip in clips)
        {
            if (GUILayout.Button(clip.name))
            {
                selectedClip = clip;
                selectedTab = 0;
            }
        }
        EditorGUILayout.EndScrollView();
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
}
