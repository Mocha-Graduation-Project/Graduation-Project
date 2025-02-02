using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class Player : MonoBehaviour
{
    public static Player Instance;  
    [SerializeField] private PlayerInput MoveAction;
    [SerializeField] private float MoveSpeed;
    public Vector2 InputMove = Vector2.zero;
    [SerializeField] private float jumpPower;
    private Rigidbody2D rb;
    [SerializeField] private GameObject Bullets;
    [SerializeField] private GameObject ShotPosition;
    [SerializeField] private GameObject AttackCollision;
    [NonSerialized]public int direction = 1;
    [SerializeField]private float MaxBulletTime;
    [NonSerialized] public float BulletTime = 0;
    [SerializeField]private Image BulletUI;
    private bool isJump = true;
    public GameObject Arrow;
    public bool isMove = true;
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        MoveAction.actions["Move"].performed += OnMove;
        MoveAction.actions["Move"].canceled += OnMove;
        MoveAction.actions["Jump"].started += OnJump;
        MoveAction.actions["Shot"].started += OnShot;
        MoveAction.actions["Attack"].started += OnAttack;
        rb = GetComponent<Rigidbody2D>();
        Arrow.SetActive(false);
    }
    void Update()
    {
        BulletUI.fillAmount = (MaxBulletTime - BulletTime) / MaxBulletTime;

        if (!GetComponent<Renderer>().isVisible)
        {
            Vector3 pos = transform.position;
            if(pos.x <0)
                transform.position = new Vector3(9.11f, pos.y, pos.z);
            else
                transform.position =  new Vector3(-9.11f,pos.y,pos.z);
        }

        if(BulletTime >0)
            BulletTime-=Time.deltaTime;
        if (!isMove)
            return;
        if (InputMove.x < 0)
        {
            transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            direction = -1;
        }
        else if (InputMove.x > 0)
        {
            transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            direction = 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
            isJump = true;
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        InputMove = context.ReadValue<Vector2>();
        float Angle = Mathf.Atan2(InputMove.y, InputMove.x) * Mathf.Rad2Deg;
        Arrow.transform.rotation = Quaternion.Euler(0f, 0f, Angle);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(isJump)
        {
            rb.AddForce(new Vector2(0, jumpPower));
            isJump = false;
        }

    }

    private void OnShot(InputAction.CallbackContext context)
    {
        if (BulletTime <= 0)
        {
            GameObject bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            Bullet bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
            BulletTime = MaxBulletTime;
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        AttackCollision.gameObject.SetActive(true);
        Invoke("AttackFinish", 0.3f);
    }
    private void AttackFinish()
    {
        AttackCollision.gameObject.SetActive(false);
    }
}
