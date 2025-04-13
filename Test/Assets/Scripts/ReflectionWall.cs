using UnityEngine;

public class ReflectionWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Bullet bullet))
        {
            Vector3 currentPower = bullet.GetPower();
            float speed = currentPower.magnitude; // 現在の速さ（ベクトルの大きさ）

            Vector3 normal = transform.up.normalized; // 壁の面に垂直な方向（法線）

            Vector3 reflectPower = normal * speed;

            bullet.SetPower(reflectPower);
            bullet.OnReflect();
        }
    }
}