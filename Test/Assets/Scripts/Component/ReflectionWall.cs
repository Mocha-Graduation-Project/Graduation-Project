using Player;
using Scripts;
using UnityEngine;

namespace Component
{
    /*
     弾を反対方向に弾き返すscript
     主にステージギミックとして使う
     */
    public class ReflectionWall : MonoBehaviour
    {
        /*
         一定ダメージ受けると機能停止して弾を通すようにして、一定時間後に元に戻す
         ボスReflectionBeeに移動したためHPを現在使用していないが今後使用する可能性がある為残す
        */
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
                // 壁の法線ベクトル (このオブジェクトの「上」方向) を渡す
                Vector3 normal = transform.up; 

                // BulletのReflectFromWall関数を呼び出し、反射が成功したかを受け取る
                bool didReflect = bullet.ReflectFromWall(normal);

                // 反射に成功した場合（クールダウン中でなかった場合）のみ、HPを減らす
                if (didReflect)
                {
                    shieldHP -= bullet.Damage;
                    
                    //特定の時間盾を無効化する
                    if (shieldHP <= 0)
                    {
                        shieldHP = 0;
                        this.gameObject.SetActive(false);
                        Invoke("ShieldReset", revivaltime);
                    }
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