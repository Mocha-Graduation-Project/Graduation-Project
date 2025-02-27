using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField] private PlayerInput MoveAction;
    [SerializeField] private float MoveSpeed;
    public Vector2 InputMove = Vector2.zero;
    [SerializeField] private float jumpPower;
    [SerializeField] private GameObject Bullets;
    [SerializeField] private GameObject ShotPosition;
    [SerializeField] private GameObject AttackCollision;
    [SerializeField] private GameObject QuickAttackCollision;
    [SerializeField] private float MaxBulletTime;
    [SerializeField] private Image BulletUI;
    [SerializeField] private Image ReflectionUI;
    public GameObject Arrow;
    public bool isMove = true;
    [SerializeField] private int MaxJumpCount;
    [SerializeField] private float MaxJumpHeight;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip ReflectionSound;
    [SerializeField] private AudioClip ShotSound;
    [SerializeField] private AudioClip DamageSound;
    private AudioSource audioSource;
    [NonSerialized] public float BulletTime;
    [NonSerialized] public int direction = 1;
    private bool isfirst = true;
    private bool isJump;
    private int jumpCount;
    private bool isGround;
    private Rigidbody2D rb;
    private float startY;
    [SerializeField] private float MaxReflectionTime ;
    private float ReflectionTime = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        MoveAction.actions["Move"].performed += OnMove;
        MoveAction.actions["Move"].canceled += OnMove;
        MoveAction.actions["Jump"].started += OnJump;
        MoveAction.actions["Shot"].started += OnShot;
        MoveAction.actions["Attack"].performed += OnAttack;
        MoveAction.actions["Jump"].canceled += OffJump;
        MoveAction.actions["QuickAttack"].performed += OnQuickAttack;

        rb = GetComponent<Rigidbody2D>();
        Arrow.SetActive(false);
        jumpCount = MaxJumpCount;
        audioSource = GetComponent<AudioSource>();

    }

    private void Update()
    {
        BulletUI.fillAmount = (MaxBulletTime - BulletTime) / MaxBulletTime;
        ReflectionUI.fillAmount = (MaxReflectionTime - ReflectionTime) / MaxReflectionTime;

        if (!GetComponent<Renderer>().isVisible)
        {
            if (isfirst)
            {
                isfirst = false;
            }
            else
            {
                var pos = transform.position;
                if (pos.x < 0)
                    transform.position = new Vector3(7f, pos.y, pos.z);
                else
                    transform.position = new Vector3(-7f, pos.y, pos.z);
            }
        }

        if (BulletTime > 0)
            BulletTime -= Time.deltaTime;
        if (!isMove)
            return;
        if (InputMove.x < 0)
        {
            transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
            transform.localScale = new Vector3(1f, 1f, -1f);
            direction = -1;
        }
        else if (InputMove.x > 0)
        {
            transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
            transform.localScale = new Vector3(1f, 1f, 1f);
            direction = 1;
        }

        if (isJump)
        {
            if (transform.position.y - startY < MaxJumpHeight)
                rb.linearVelocityY = jumpPower;
            else
                isJump = false;
        }

        if (ReflectionTime > 0)
            ReflectionTime -= Time.deltaTime;
        animator.SetFloat("Jump", rb.linearVelocityY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            jumpCount = MaxJumpCount;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        animator.SetBool("isMove", true);
        InputMove = context.ReadValue<Vector2>();
        if (InputMove != Vector2.zero)
            animator.SetBool("isMove", true);
        else
            animator.SetBool("isMove", false);
        var Angle = Mathf.Atan2(InputMove.y, InputMove.x) * Mathf.Rad2Deg;
        Arrow.transform.rotation = Quaternion.Euler(0f, 0f, Angle);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (jumpCount > 0)
        {
            isJump = true;
            startY = transform.position.y;
            jumpCount--;
            animator.SetBool("isJump", true);
        }
    }

    public void OffJump(InputAction.CallbackContext context)
    {
        isJump = false;
        animator.SetBool("isJump", false);
    }

    public void OnShot(InputAction.CallbackContext context)
    {
        if (BulletTime <= 0)
        {
            audioSource.PlayOneShot(ShotSound);
            var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            var bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
            BulletTime = MaxBulletTime;
            animator.SetTrigger("isShot");
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(ReflectionTime <= 0)
        {
            AttackCollision.gameObject.SetActive(true);
            ReflectionTime = MaxReflectionTime;
        }
           
        else
            QuickAttackCollision.gameObject.SetActive(true);
        Invoke("AttackFinish", 0.3f);
        animator.SetTrigger("isAttack");
    }

    public void OnQuickAttack(InputAction.CallbackContext context)
    {
        QuickAttackCollision.gameObject.SetActive(true);
        Invoke("AttackFinish", 0.3f);
        animator.SetTrigger("isAttack");
    }

    public void AttackFinish()
    {
        AttackCollision.gameObject.SetActive(false);
        QuickAttackCollision.gameObject.SetActive(false);
    }

    public void PlayReflectionSound()
    {
        audioSource.PlayOneShot(ReflectionSound);
    }

    public void PlayDamageSound()
    {
        audioSource.PlayOneShot(DamageSound);
    }
}