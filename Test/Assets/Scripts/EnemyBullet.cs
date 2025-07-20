using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Scripts
{
    public class EnemyBullet : MonoBehaviour
    {
        Player player => Player.Instance;
        private UnityEngine.Vector3 PowerDirection;
        [SerializeField] private float Power;
        private float correctionAimPos = 1.5f;

        public void SetPower(UnityEngine.Vector3 Pos)
        {
            float Angle = Mathf.Atan2(player.gameObject.transform.position.y - Pos.y + correctionAimPos,
                player.gameObject.transform.position.x - Pos.x);
            //Debug.Log("EB角度:"+Angle);
            UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0).normalized;
            PowerDirection = direction * Power;
        }

        public void SetStraightPower(UnityEngine.Vector3 angle)
        {
            UnityEngine.Vector3 direction=Vector3.zero;

            if (angle.z >= 0 && angle.z <= 45)
            {
                direction = Vector3.left;
            }
            else if (angle.z > 45 && angle.z <= 135)
            {
                direction = Vector3.down;
            }
            else if (angle.z > 135 && angle.z <= 180)
            {
                direction = Vector3.right;
            }
            else
            {
                Debug.Log("範囲外");
            }
            PowerDirection = direction * Power;
        }

        void Update()
        {
            transform.position += PowerDirection * Time.deltaTime;

            if (!GetComponent<Renderer>().isVisible)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Player")
            {
                if (collision.TryGetComponent<PlayerStatus>(out PlayerStatus status))
                {
                    status.Damage(1);
                }

                Destroy(this.gameObject);
            }

        }
    }
}
