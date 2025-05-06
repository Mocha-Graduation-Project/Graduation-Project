using System;
using UnityEngine;
namespace Scripts
{
    public class WeakAttackReflector : MonoBehaviour
    {
        string playerBulletTag = "Bullet";
        [SerializeField,JapaneseLabel("〇以下の弱い弾を跳ね返す")] int destroyBulletCount = 1;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(playerBulletTag) == true)
            {
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                int count = bullet.reflectionCount;

                if (count <= destroyBulletCount)
                {
                    // 弱い弾は反射
                    Vector3 reversePower = -bullet.GetPower().normalized * bullet.GetPower().magnitude;
                    bullet.SetPower(reversePower);
                    bullet.OnReflect();
                }
            }
        }
    }
}