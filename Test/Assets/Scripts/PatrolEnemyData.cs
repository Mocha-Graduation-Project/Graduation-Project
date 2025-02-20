using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PatrolEnemyData")]
public class PatrolEnemyData : ScriptableObject
{
    public enum EnemyState
    {
        vertical,
        horizontal
    };

    [SerializeField] private EnemyState state;
    [SerializeField] private float minVerticalPos;
    [SerializeField] private float maxVerticalPos;
    [SerializeField] private float minHorizontalPos;
    [SerializeField] private float maxHorizontalPos;
    
    public EnemyState State{get{return state;}}
    public float MinVerticalPos{get{return minVerticalPos;}}
    public float MaxVerticalPos{get{return maxVerticalPos;}}
    public float MinHorizontalPos{get{return minHorizontalPos;}}
    public float MaxHorizontalPos{get{return maxHorizontalPos;}}
}
