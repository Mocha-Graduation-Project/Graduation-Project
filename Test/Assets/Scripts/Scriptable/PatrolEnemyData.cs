using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObject/PatrolEnemyData")]
public class PatrolEnemyData : ScriptableObject
{
    public enum EnemyState
    {
        vertical,
        horizontal
    };

    [SerializeField] private EnemyState state;
    [SerializeField] private float leftRange;
    [SerializeField] private float rightRange;
    [SerializeField] private float upRange;
    [SerializeField] private float downRange;
    
    public EnemyState State{get{return state;}}
    public float LeftRange{get{return leftRange;}}
    public float RightRange{get{return rightRange;}}
    public float UpRange{get{return upRange;}}
    public float DownRange{get{return downRange;}}
}
