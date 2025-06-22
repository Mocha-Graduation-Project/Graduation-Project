using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine.InputSystem;
using UnityEngine;
using Scripts;
using Scripts.Scriptable;

namespace Scripts
{
    public class Bullet : MonoBehaviour
    { 
        private UnityEngine.Vector3 power;
        Player player => Player.Instance;
        [NonSerialized]public float PowerDirection;
        private int count = 1;
        private bool isAttack = false;
        private bool isQuick = false;
        
        private Material material;
        private bool destroyed = false; //Destroyで消してもAttckに反応することがあるので仮で配置、バグ治せれば消す
        
        [SerializeField] private MeshRenderer meshRendererChild;
        [NonSerialized]public int reflectionCount;
        private int maxReflectionCount = 4;

        private UnityEngine.Vector3 SavePower;
        private Vector2 lastInputDirection = Vector2.right;
        
        private float staminaDrainPerSecond = 0f;
        private float quickStaminaDrainPerSecond = 0f;
        
        private PlayerInput moveAction;
        
        private float attackCoolMaxTime = 1f;
        private float attackCoolTime = 0f;
        [NonSerialized][JapaneseLabel("初期ダメージ値")]public int Damage = 1;
        [JapaneseLabel("最大スピード")] private float maxBulletSpeed;
        [JapaneseLabel("最大ダメージ")] private int maxDamage;
        [SerializeField] private CharacterParams characterParams;

        private void Awake()
        {
            PlayerParamReset();
        }
        private void Start()
        {
            power *= PowerDirection;
            Debug.Log(meshRendererChild.name);
            reflectionCount = 0;
            //cameraAreaManager = GameObject.FindObjectOfType<CameraAreaManager>();
            moveAction = GetComponent<PlayerInput>();
            moveAction.actions["Attack"].canceled += OffAttack;
        }

        void Update()
        {
            // 最大スピード制限
            if (power.magnitude > maxBulletSpeed)
            {
                power = power.normalized * maxBulletSpeed;
            }
            
            transform.position += power * Time.deltaTime;
            
            if (power != Vector3.zero)
            {
                float angle = Mathf.Atan2(power.y, power.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            
            Vector2 input = player.InputMove;

            // 入力があれば更新、なければ前回の方向を維持
            if (input.sqrMagnitude > 0.01f)
            {
                lastInputDirection = input.normalized;
            }
            
            //スタミナ消費
            if (isAttack)
            {
                player.currentStamina -= staminaDrainPerSecond * Time.deltaTime * 5;
                if (player.currentStamina <= 0)
                {
                    player.currentStamina = 0;
                    player.AttackFinish();
                    if (attackCoolTime > attackCoolMaxTime)
                    {
                        Attack(); 
                        attackCoolTime = 0;
                    }

                }
            }

            if (isQuick)
            {
                player.currentStamina -= quickStaminaDrainPerSecond * Time.deltaTime * 5;
                if (player.currentStamina <= 0)
                {
                    player.currentStamina = 0;
                    player.AttackFinish();
                    if (attackCoolTime > attackCoolMaxTime)
                    {
                        //QuickAttack(); 
                        attackCoolTime = 0;
                    }

                }
            }
            
            
            attackCoolTime+= Time.deltaTime;
            player.staminaSlider.value = player.currentStamina;
            
        }

        private void PlayerParamReset()
        {
            Damage = characterParams.damage;
            maxBulletSpeed = characterParams.maxBulletSpeed;
            maxDamage = characterParams.maxDamage;
            power = characterParams.power;
            staminaDrainPerSecond = characterParams.staminaDrainPerSecond;
            quickStaminaDrainPerSecond =  characterParams.quickStaminaDrainPerSecond;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Ground" || (collision.gameObject.tag == "Player" && !isAttack))
            {
                if (collision.TryGetComponent<PlayerStatus>(out PlayerStatus status))
                {
                    status.Damage(1);
                }

                ResetBullet();
                Time.timeScale = 1f;
                player.isMove = true;
                isAttack = false;
                destroyed = true;
                Destroy(this.gameObject);
            }

            if (collision.gameObject.tag == "Attack" && !destroyed)
            {
                
                player.isMove = false;
                player.Arrow.SetActive(true);
                isAttack = true;
                Time.timeScale = 0.2f;
                power = UnityEngine.Vector3.zero;
                //Invoke("Attack", 0.3f);

            }

            if (collision.gameObject.tag == "QuickAttack" && !destroyed)
            {
                player.isMove = false;
                isQuick = true;
                SavePower = -power;
                //Power = UnityEngine.Vector3.zero;
                QuickAttack();

            }
        }

        private void Attack()
        {
            if (this.gameObject.CompareTag("EnemyBullet"))
                this.gameObject.tag = "Bullet";
            
            Damage = Mathf.Min(Damage * 2, maxDamage);
            
            //powerlevelの変更をここに入れたい
            reflectionCount++;
            float powerColor = reflectionCount * 0.26f;
            if (reflectionCount >= maxReflectionCount)
                powerColor = 1.0f;

            meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
            player.currentStamina -= 2.5f;
            player.Arrow.SetActive(false);
            player.isMove = true;
            Invoke("AttackFalse", 0.2f);
            PowerDirection *= 1.25f;
            if (PowerDirection < 0)
                PowerDirection *= -1;
            
            float Angle = Mathf.Atan2(lastInputDirection.y, lastInputDirection.x);
            UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0);
            power = direction * PowerDirection * 10f;
            
            Time.timeScale = 1f;
            player.PlayReflectionSound();
        }

        private void QuickAttack()
        {
            if (this.gameObject.CompareTag("EnemyBullet"))
                this.gameObject.tag = "Bullet";
            
            Damage = Mathf.Min(Damage * 2, maxDamage);
            
            //powerlevelの変更をここに入れたい
            reflectionCount++;
            
            float powerColor = reflectionCount * 0.26f;
            if (reflectionCount >= maxReflectionCount)
                powerColor = 1.0f;
            
            meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
            player.isMove = true;
            Invoke("AttackFalse", 0.2f);
            Vector3 reversePower = -1 * GetPower().normalized * GetPower().magnitude;
            SetPower(reversePower);

            // PowerDirection *= 1.25f;
            // if (PowerDirection < 0)
            //     PowerDirection *= -1;
            //
            // Power = SavePower * PowerDirection;
            player.PlayReflectionSound();
        }

        private void OffAttack(InputAction.CallbackContext context)
        {
            //CancelInvoke("Attack");
            if (isAttack)
            {
                Attack();
            }

            // if (isQuick)
            // {
            //     QuickAttack();
            // }
        }
        public Vector3 GetPower()
        {
            return power;
        }
        
        public void SetPower(Vector3 newPower)
        {
            power = newPower;
        }
        
        public void SetPowerEnemy(Vector3 Pos)
        {
            float correctionAimPos = 0.5f;
            float Angle = Mathf.Atan2(player.gameObject.transform.position.y - Pos.y + correctionAimPos,
                player.gameObject.transform.position.x - Pos.x);
            //Debug.Log(Angle);
            Vector3 direction = new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0).normalized;
            power = direction * 5f;
            PowerDirection = 1f;
            Debug.Log("Pos:"+Pos+"/Power:"+power);;
        }

        public void OnReflect()
        {
            player.PlayReflectionSound();
        }
        public void AttackFalse()
        {
            isAttack = false;
            isQuick = false;
        }
        public void ResetBullet()
        {
            moveAction.actions["Attack"].canceled -= OffAttack;
        }
    }
}
