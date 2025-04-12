using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;
using System.Collections.Generic;

[Serializable]
public class VFXEntry
{
    public string name;
    public GameObject vfxObject;
}
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
    public GameObject Arrow;
    public bool isMove = true;
    [SerializeField] private int MaxJumpCount;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip ReflectionSound;
    [SerializeField] private AudioClip ShotSound;
    [SerializeField] private AudioClip DamageSound;

    [SerializeField] [JapaneseLabel("2回目のジャンプまでのクールタイム")]
    private float jumpCooldown = 0.2f;

    private AudioSource audioSource;
    [NonSerialized] public float BulletTime;
    [NonSerialized] public int direction = 1;
    private bool isfirst = true;
    private bool isGround;
    private bool isJump;
    private int jumpCount;
    private float lastJumpTime; // 最後にジャンプした時間
    private Rigidbody2D rb;
    private float startY;
    [SerializeField] CameraAreaManager cameraAreaManager;
    [SerializeField] MapManager mapManager;
    [SerializeField] SceneButtonManager sceneButtonManager;
    [SerializeField] private float maxFallSpeed = 20f;

    [SerializeField] private List<VFXEntry> vfxEntries = new List<VFXEntry>();
    private Dictionary<string, GameObject> vfxDictionary = new Dictionary<string, GameObject>();

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
       // MoveAction.actions["Jump"].started += OnJump;
        MoveAction.actions["Shot"].started += OnShot;
        MoveAction.actions["Attack"].performed += OnAttack;
      //  MoveAction.actions["Jump"].canceled += OffJump;
        MoveAction.actions["QuickAttack"].performed += OnQuickAttack;

        rb = GetComponent<Rigidbody2D>();
        Arrow.SetActive(false);
        jumpCount = MaxJumpCount;
        audioSource = GetComponent<AudioSource>();
        
        cameraAreaManager = GameObject.FindObjectOfType<CameraAreaManager>();
        mapManager = GameObject.FindObjectOfType<MapManager>();
        sceneButtonManager = GameObject.FindObjectOfType<SceneButtonManager>();
        
        foreach (var entry in vfxEntries)
        {
            if (!vfxDictionary.ContainsKey(entry.name))
            {
                vfxDictionary.Add(entry.name, entry.vfxObject);
            }
        }
    }

    private void FixedUpdate()
    {
        BulletUI.fillAmount = (MaxBulletTime - BulletTime) / MaxBulletTime;

        if (!GetComponent<Renderer>().isVisible)
        {
            if (isfirst)
            {
                isfirst = false;
            }
            else
            {
                Vector3 pos = transform.position;
                
                if (pos.x < cameraAreaManager.LeftMax)
                    pos.x = cameraAreaManager.RightMax;
                else if(pos.x > cameraAreaManager.RightMax)
                    pos.x = cameraAreaManager.LeftMax;

                if (pos.y < cameraAreaManager.DownMax)
                {
                    if (mapManager.CanLoop(pos, MapManager.Side.down) == true)
                    {
                        pos.y = cameraAreaManager.UpMax;
                    }
                    else
                    {
                        pos.y = cameraAreaManager.DownMax;
                    }

                    // Debug.Log(rb.linearVelocity);
                    if (rb.linearVelocity.y < maxFallSpeed * -1)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed * -1);
                    }
                }
                else if (pos.y > cameraAreaManager.UpMax)
                {
                    if (mapManager.CanLoop(pos, MapManager.Side.up) == true)
                    {
                        pos.y = cameraAreaManager.DownMax;
                    }
                    else
                    {
                        pos.y = cameraAreaManager.UpMax;
                    }
                }

                transform.position = pos;
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

        if (InputMove.y != 0)
        {
            transform.position += new Vector3(0, MoveSpeed * InputMove.y, 0) * Time.deltaTime;
            
        }
       

        animator.SetFloat("Jump", rb.linearVelocityY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            jumpCount = MaxJumpCount;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (sceneButtonManager.CurrentState != SceneButtonManager.State.Gameplay) return;
        
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
        if (sceneButtonManager.CurrentState != SceneButtonManager.State.Gameplay) return;
        
        if (jumpCount > 0 && Time.time - lastJumpTime >= jumpCooldown)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpCount--;
            lastJumpTime = Time.time;
            animator.SetTrigger("isJump");
        }
    }

    public void OffJump(InputAction.CallbackContext context)
    {
        if (sceneButtonManager.CurrentState != SceneButtonManager.State.Gameplay) return;
        
        isJump = false;
        animator.SetBool("isJump", false);
    }

    public void OnShot(InputAction.CallbackContext context)
    {
        if (sceneButtonManager.CurrentState != SceneButtonManager.State.Gameplay) return;
        
        if (BulletTime <= 0)
        {
            // audioSource.PlayOneShot(ShotSound);
            // var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            // var bullet = bullets.GetComponent<Bullet>();
            // bullet.PowerDirection = direction;
            BulletTime = MaxBulletTime;
            animator.SetTrigger("isShot");
        }
    }

    public void Shot()
    {
        audioSource.PlayOneShot(ShotSound);
        var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
        var bullet = bullets.GetComponent<Bullet>();
        bullet.PowerDirection = direction;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (sceneButtonManager.CurrentState != SceneButtonManager.State.Gameplay) return;
        
        AttackCollision.gameObject.SetActive(true);
        Invoke("AttackFinish", 0.3f);
        animator.SetTrigger("isAttack");
    }

    public void OnQuickAttack(InputAction.CallbackContext context)
    {
        if (sceneButtonManager.CurrentState != SceneButtonManager.State.Gameplay) return;
        
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
        animator.Play("Damage");
        audioSource.PlayOneShot(DamageSound);
    }
    public void TriggerVFX(string vfxName)
    {
        if (vfxDictionary.TryGetValue(vfxName, out var vfxObject))
        {
            if (vfxObject.TryGetComponent<VisualEffect>(out var vfx))
            {
                vfx.SendEvent("OnPlay"); // VFX のイベントを送信
            }
            else
            {
                Debug.LogWarning($"指定されたVFXオブジェクト '{vfxName}' に VisualEffect コンポーネントがありません。");
            }
        }
        else
        {
            Debug.LogWarning($"VFX '{vfxName}' が見つかりません。");
        }
    }
}