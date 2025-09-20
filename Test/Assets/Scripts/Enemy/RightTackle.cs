using UnityEngine;

public class RightTackle : MonoBehaviour, IState
{
    private readonly EnemyAI enemyAI;
    public RightTackle(EnemyAI enemyAI)
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
