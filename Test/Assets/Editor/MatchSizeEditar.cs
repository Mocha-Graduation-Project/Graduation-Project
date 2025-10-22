#region 
using UnityEngine;
using UnityEditor;
using System.Linq;
#endregion
public class MatchSizeEditor : Editor
{
    [MenuItem("GameObject/Match/サイズを合わせる")]
    private static void MatchScale()
    {
        // 選択されているすべてのGameObjectを取得
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length < 2)
        {
            Debug.LogWarning("サイズを合わせるには、少なくとも2つのオブジェクトを選択してください。");
            return;
        }

        // 最初のオブジェクトを基準（コピー元）とする
        Transform sourceTransform = selectedObjects[0].transform;
        Vector3 targetScale = sourceTransform.localScale;

        // 2つ目以降のオブジェクトのスケールを変更
        for (int i = 1; i < selectedObjects.Length; i++)
        {
            Transform targetTransform = selectedObjects[i].transform;
            
            // Undoのために変更を記録
            Undo.RecordObject(targetTransform, "Match Scale");
            
            // スケールを合わせる
            targetTransform.localScale = targetScale;
        }
        
    }
    [MenuItem("GameObject/Match/Y座標を合わせる")]
    private static void FlattenYPosition()
    {
        // 選択されているすべてのGameObjectを取得
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length < 2)
        {
            Debug.LogWarning("整地するには、少なくとも2つのオブジェクトを選択してください。");
            return;
        }

        // 最初のオブジェクトを基準（コピー元）とする
        Transform sourceTransform = selectedObjects[0].transform;
        float targetY = sourceTransform.position.y;

        // 2つ目以降のオブジェクトのY座標を変更
        for (int i = 1; i < selectedObjects.Length; i++)
        {
            Transform targetTransform = selectedObjects[i].transform;
            
            // Undoのために変更を記録
            Undo.RecordObject(targetTransform, "Flatten Y Position");
            
            // Y座標を合わせる
            Vector3 newPosition = targetTransform.position;
            newPosition.y = targetY;
            targetTransform.position = newPosition;
        }
    }

    [MenuItem("GameObject/Match/X座標を合わせる")]
    private static void FlattenXPosition()
    {
        // 選択されているすべてのGameObjectを取得
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length < 2)
        {
            Debug.LogWarning("整地するには、少なくとも2つのオブジェクトを選択してください。");
            return;
        }
        
        // 最初のオブジェクトを基準（コピー元）とする
        Transform sourceTransform = selectedObjects[0].transform;
        float targetX = sourceTransform.position.x;
        for (int i = 1; i < selectedObjects.Length; i++)
        {
            Transform targetTransform = selectedObjects[i].transform;
            
            // Undoのために変更を記録
            Undo.RecordObject(targetTransform, "Flatten X Position");
            
            // X座標を合わせる
            Vector3 newPosition = targetTransform.position;
            newPosition.x = targetX;
            targetTransform.position = newPosition;
        }
    }

    [MenuItem("GameObject/Match/Z座標を合わせる")]
    private static void FlattenZPosition()
    {
        // 選択されているすべてのGameObjectを取得
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length < 2)
        {
            Debug.LogWarning("整地するには、少なくとも2つのオブジェクトを選択してください。");
            return;
        }
        
        // 最初のオブジェクトを基準（コピー元）とする
        Transform sourceTransform = selectedObjects[0].transform;
        float targetZ = sourceTransform.position.z;
        for (int i = 1; i < selectedObjects.Length; i++)
        {
            Transform targetTransform = selectedObjects[i].transform;
            
            // Undoのために変更を記録
            Undo.RecordObject(targetTransform, "Flatten Z Position");
            
            // Z座標を合わせる
            Vector3 newPosition = targetTransform.position;
            newPosition.z = targetZ;
            targetTransform.position = newPosition;
        }
    }
}