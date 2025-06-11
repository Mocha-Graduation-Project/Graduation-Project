using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine.InputSystem;
using UnityEngine;
using Scripts;

namespace Scripts
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Vector3 Power;
        Player player => Player.Instance;
        public float PowerDirection;
        private int count = 1;
        private bool isAttack = false;
        public int Damage = 1;
        private Material material;

        private bool destroyed = false; //Destroyで消してもAttckに反応することがあるので仮で配置、バグ治せれば消す
        
        [SerializeField] private MeshRenderer meshRendererChild;
        public int reflectionCount;
        private int maxReflectionCount = 4;

        private UnityEngine.Vector3 SavePower;
        private Vector2 lastInputDirection = Vector2.right;
        
        private float staminaDrainPerSecond = 0f;
        
        private PlayerInput MoveAction;
        
        private float attackCoolMaxTime = 1f;
        private float attackCoolTime = 0f;
        
        public float maxBulletSpeed;
        private void Start()
        {
            Power *= PowerDirection;
            Debug.Log(meshRendererChild.name);
            reflectionCount = 0;
            //cameraAreaManager = GameObject.FindObjectOfType<CameraAreaManager>();
            staminaDrainPerSecond = player.staminaDrainPerSecond;
            MoveAction = GetComponent<PlayerInput>();
            MoveAction.actions["Attack"].canceled += OffAttack;
        }

        void Update()
        {
            transform.position += Power * Time.deltaTime;
            Vector2 input = player.InputMove;

            // 入力があれば更新、なければ前回の方向を維持
            if (input.sqrMagnitude > 0.01f)
            {
                lastInputDirection = input.normalized;
            }
            
            //スタミナ消費
            if (isAttack)
            {
                player.currentStamina -= staminaDrainPerSecond * Time.deltaTime*5;
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
            attackCoolTime+= Time.deltaTime;
            player.staminaSlider.value = player.currentStamina;
            
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
                Power = UnityEngine.Vector3.zero;
                //Invoke("Attack", 0.3f);

            }

            if (collision.gameObject.tag == "QuickAttack" && !destroyed)
            {
                player.isMove = false;
                isAttack = true;
                SavePower = -Power;
                //Power = UnityEngine.Vector3.zero;
                QuickAttack();

            }
        }

        private void Attack()
        {
            if (this.gameObject.CompareTag("EnemyBullet"))
                this.gameObject.tag = "Bullet";
            Damage *= 2;
            //powerlevelの変更をここに入れたい
            reflectionCount++;
            float powerColor = reflectionCount * 0.26f;
            if (reflectionCount >= maxReflectionCount)
                powerColor = 1.0f;

            meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
            player.currentStamina -= 2.5f;
            player.Arrow.SetActive(false);
            player.isMove = true;
            Invoke("AttckFalse", 0.2f);
            PowerDirection *= 1.25f;
            if (PowerDirection < 0)
                PowerDirection *= -1;
            
            float Angle = Mathf.Atan2(lastInputDirection.y, lastInputDirection.x);
            UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0);
            Power = direction * PowerDirection * 10f;
            Time.timeScale = 1f;
            player.PlayReflectionSound();
        }

        private void QuickAttack()
        {
            if (this.gameObject.CompareTag("EnemyBullet"))
                this.gameObject.tag = "Bullet";
            Damage *= 2;
            //powerlevelの変更をここに入れたい
            reflectionCount++;
            
            float powerColor = reflectionCount * 0.26f;
            if (reflectionCount >= maxReflectionCount)
                powerColor = 1.0f;
            
            meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
            player.isMove = true;
            Invoke("AttckFalse", 0.2f);
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
        }
        public Vector3 GetPower()
        {
            return Power;
        }
        
        public void SetPower(Vector3 newPower)
        {
            Power = newPower;
        }
        
        public void SetPowerEnemy(Vector3 Pos)
        {
            float correctionAimPos = 0.5f;
            float Angle = Mathf.Atan2(player.gameObject.transform.position.y - Pos.y + correctionAimPos,
                player.gameObject.transform.position.x - Pos.x);
            //Debug.Log(Angle);
            Vector3 direction = new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0).normalized;
            Power = direction * 5f;
            PowerDirection = 1f;
            Debug.Log("Pos:"+Pos+"/Power:"+Power);;
        }

        public void OnReflect()
        {
            player.PlayReflectionSound();
        }
        public void AttckFalse()
        {
            isAttack = false;
        }
        public void ResetBullet()
        {
            MoveAction.actions["Attack"].canceled -= OffAttack;
        }
    }
}
