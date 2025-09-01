using System;
using UnityEngine;
using System.Collections;
namespace Scripts
{
    public class WeakAttackReflector : MonoBehaviour
    {
        string playerBulletTag = "Bullet";
        [SerializeField,JapaneseLabel("〇以下の弱い弾を跳ね返す")] int destroyBulletCount = 1;
        private Collider2D parentObjects;
        
        private void Awake()
        {
            parentObjects = transform.parent.GetComponent<Collider2D>();
            parentObjects.enabled = false;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(playerBulletTag))
            {
                Bullet bullet = collision.GetComponent<Bullet>();
                int count = bullet.reflectionCount;

                if (count <= destroyBulletCount)
                {
                    // 弱い弾は反射
                    Vector3 originalPower = bullet.GetPower();
                    Vector3 reversedDirection = -originalPower.normalized;
                    float speed = originalPower.magnitude;
                    
                    bullet.SetDirection(reversedDirection);
                    bullet.SetSpeed(speed);
                    bullet.UpdatePower();

                    bullet.OnReflect();
                }
                else
                {
                    // 強い弾は貫通 → 一時的に親のコライダーを有効にする
                    parentObjects.enabled = true;
                    Invoke(nameof(ParentCollider), 0.5f);
                }
            }
        }

        private void ParentCollider()
        {
            parentObjects.enabled = false;
        }
    }
}