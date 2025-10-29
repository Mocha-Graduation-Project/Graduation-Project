using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyAI))]
#endif

public class NomalEnemy : EnemyAI
{
    override protected void Update()
    {
        base.Update();
        //stateMachine.Update();
    }
}
