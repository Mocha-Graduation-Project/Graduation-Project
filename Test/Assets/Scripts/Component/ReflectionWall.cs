using Player;
using Scripts;
using UnityEngine;

namespace Component
{

    public class ReflectionWall : MonoBehaviour
    {
        //一定ダメージ受けると機能停止して弾を通すようにして、一定時間後に元に戻す
        [SerializeField] [JapaneseLabel("盾のHP")] private float shieldMaxHP;
        [SerializeField] [JapaneseLabel("盾の現在HP")]  private float shieldHP;

        [SerializeField] [JapaneseLabel("復活までの時間")] private float revivaltime;

        void Start()
        {
            shieldHP = shieldMaxHP;
        }
        
        private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out Bullet bullet))
            {
                Vector3 incomingPower = bullet.GetPower();
                float speed = incomingPower.magnitude;

                Vector3 normal = transform.up.normalized;

                // Vector3.Reflectで反射ベクトルを求める
                Vector3 reflectedDirection = Vector3.Reflect(incomingPower.normalized, normal);

                // Bulletに新しい方向とスピードを設定
                bullet.SetDirection(reflectedDirection);
                bullet.SetSpeed(speed);
                bullet.UpdatePower();

                bullet.OnReflect();

                shieldHP -= bullet.Damage;
                if (shieldHP <= 0)
                {
                    shieldHP = 0;
                    this.gameObject.SetActive(false);
                    Invoke("ShieldReset", revivaltime);
                }
            }
        }

        void ShieldReset()
        {
            shieldHP = shieldMaxHP;
            this.gameObject.SetActive(true);
        }
    }
}