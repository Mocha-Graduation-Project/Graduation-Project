using UnityEngine;

public class ShieldCollider : MonoBehaviour
{
    private enum ShielderLookPosition
    {
        Left = 0,
        Right = 1,
    }
    
    [SerializeField] private ShielderLookPosition shielderLookPosition;
    string playerBulletTag = "Bullet";
    [SerializeField] Shielder shielder;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shielder = GetComponentInParent<Shielder>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (shielder.IsSetUp == false)
        {
            Debug.Log("セットアップが完了していません");
            return;
        }

        Debug.Log("盾当たった:" + other.gameObject.name);
        
        if (other.CompareTag(playerBulletTag) == true)
        {
            Player.Bullet bullet = other.gameObject.GetComponent<Player.Bullet>();
            float speed = 0f;
            //Debug.Log("ball:" + bullet.GetPower());
            switch (shielderLookPosition)
            {
                case ShielderLookPosition.Left:
                    speed = bullet.GetPower().x;
                    break;
                case ShielderLookPosition.Right:
                    speed = bullet.GetPower().x * (-1);
                    break;
                  default:
                    break;
            }

            if (speed > 0)
            {
                shielder.BlockShield();
                Destroy(other.gameObject);
            }
        }
    }
}
