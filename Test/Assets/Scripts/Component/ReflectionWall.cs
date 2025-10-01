using Player;
using Scripts;
using UnityEngine;

namespace Component
{

    public class ReflectionWall : MonoBehaviour
    {
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
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out Bullet bullet)) return;
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
        }
    }
}