using Player;
using Scripts.Scriptable;
using UnityEngine;

namespace Enemy.Ability
{
    public class Shield : MonoBehaviour
    {
        private enum ShieldPosition
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3,
        }

        [SerializeField] private ShieldPosition shieldPosition;
        [SerializeField] private SoundData soundData;
        private AudioSource audioSource;
        private AudioClip shieldSound;
        string playerBulletTag = "Bullet";

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            shieldSound = soundData.ShieldSound;
        }
        private void OnTriggerEnter(Collider collision)
        {
            if (collision.CompareTag(playerBulletTag) == true)
            {
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                float speed = 0f;
                //Debug.Log("ball:" + bullet.GetPower());
                switch (shieldPosition)
                {
                    case ShieldPosition.Left:
                        speed = bullet.GetPower().x;
                        break;
                    case ShieldPosition.Right:
                        speed = bullet.GetPower().x * (-1);
                        break;
                    case ShieldPosition.Up:
                        speed = bullet.GetPower().y * (-1);
                        break;
                    case ShieldPosition.Down:
                        speed = bullet.GetPower().y;
                        break;
                    default:
                        break;
                }

                if (speed > 0)
                {
                    audioSource.PlayOneShot(shieldSound);
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}
