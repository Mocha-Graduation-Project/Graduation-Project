using UnityEngine;

public class ReflectionWall : MonoBehaviour
{
    [SerializeField] private float reflectPower = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Bullet bullet))
        {
            var reflectDir = transform.up.normalized;

            bullet.SetPower(reflectDir * reflectPower);
            bullet.OnReflect(); // 反射時の演出など
        }
    }
}