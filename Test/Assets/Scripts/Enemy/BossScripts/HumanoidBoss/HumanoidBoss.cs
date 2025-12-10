using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyAI))]
#endif

public class HumanoidBoss : EnemyAI
{
    private static readonly int IsShot1 = Animator.StringToHash("isShot");
    private static readonly int AttackDirection = Animator.StringToHash("AttackDirection");
    private static readonly int IsAttack = Animator.StringToHash("isAttack");
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int IsMoveHash = Animator.StringToHash("isMove");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int IsGround = Animator.StringToHash("isGround");
    AnimatorStateInfo animatorStateInfo;
    
    private Animator animator;
    private int direction = 1;
    private bool isGround = false;
    
    [SerializeField] [JapaneseLabel("足元")] private Transform groundCheck;
    private readonly float checkDistance = 0.08f;


    public override void SetUp()
    {
        animator=GetComponent<Animator>();
        this.transform.eulerAngles = new Vector3(0, 90, 0);
    }
    
    override protected void Update()
    {
        CheckGround();
        base.Update();
    }

    private void QuickAttackBoss(float angle)
    {
        int attackDirection = angle switch
        {
            >= 45 and < 135 => 0,
            >= -135 and < -45 => 2,
            >= -45 and < 45 => (direction == 1) ? 1 : 3,
            _ => (direction == -1) ? 1 : 3
        };
        
        PlayAttackAnimation(attackDirection);
    }

    private void CheckGround()
    {
        bool wasGrounded = isGround; // 前フレームの接地状態
        isGround = Physics.Raycast(groundCheck.position, Vector2.down, checkDistance, enemyData.groundLayer);
    
        // アニメーターへの通知
        animator.SetBool(IsGround, isGround);
    }
    private void PlayAttackAnimation(int attackDirection)
    {
        animator.SetInteger(AttackDirection,attackDirection);
        animator.SetTrigger(IsAttack);
    }
    public void QuickAttack()
    {
        float angle = Random.Range(0, 136);
        int x = Random.Range(0, 2);
        if (x == 1)
        {
            angle *= (-1);
        }

        Debug.Log("Angle:" + angle);
        QuickAttackBoss(angle);
    }
}
