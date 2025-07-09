using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObject/PatrolEnemyData")]
public class PatrolEnemyData : ScriptableObject
{
    public enum EnemyState
    {
        none,
        vertical,
        horizontal
    };

    [SerializeField] private EnemyState state;
    [SerializeField] [JapaneseLabel("左の最大値")]private float leftRange;
    [SerializeField] [JapaneseLabel("右の最大値")]private float rightRange;
    [SerializeField] [JapaneseLabel("真ん中から横移動にかかる時間")]private float moveHorizontalTime;
    [SerializeField] [JapaneseLabel("上の最大値")]private float upRange;
    [SerializeField] [JapaneseLabel("下の最大値")]private float downRange;
    [SerializeField] [JapaneseLabel("真ん中から縦移動にかかる時間")]private float moveVerticalTime;
    [SerializeField] [JapaneseLabel("端に着いた時の次の移動までのクールタイム")]private float waitTime;
    
    public EnemyState State{get{return state;}}
    public float LeftRange{get{return leftRange;}}
    public float RightRange{get{return rightRange;}}
    public float MoveHorizontalTime{get{return moveHorizontalTime;}}
    public float UpRange{get{return upRange;}}
    public float DownRange{get{return downRange;}}
    public float MoveVerticalTime{get{return moveVerticalTime;}}
    public float WaitTime{get{return waitTime;}}

    public void ChangeState(EnemyState nextState)
    {
        state = nextState;
    }
}
