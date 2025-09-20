using UnityEngine;

public class FallingAttack : MonoBehaviour, IState
{
    private readonly EnemyAI enemyAI;
    public FallingAttack(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        
    }

    public void Execute()
    {
        
    }
    
    public void Exit()
    {
        
    }
}
