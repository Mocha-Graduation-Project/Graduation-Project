using System;
using Scripts;
using Scripts.Scriptable;
using UnityEngine;

public class Shield : MonoBehaviour
{
    enum ShieldPosition
    {
        left = 0,
        right = 1,
        up = 2,
        down = 3,
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
                case ShieldPosition.left:
                    speed = bullet.GetPower().x;
                    break;
                case ShieldPosition.right:
                    speed = bullet.GetPower().x * (-1);
                    break;
                case ShieldPosition.up:
                    speed = bullet.GetPower().y * (-1);
                    break;
                case ShieldPosition.down:
                    speed = bullet.GetPower().y;
                    break;
                default:
                    break;
            }

            if (speed > 0)
            {
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerBulletTag) == true)
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            float speed = 0f;
            //Debug.Log("ball:" + bullet.GetPower());
            switch (shieldPosition)
            {
                case ShieldPosition.left:
                    speed = bullet.GetPower().x;
                    break;
                case ShieldPosition.right:
                    speed = bullet.GetPower().x * (-1);
                    break;
                case ShieldPosition.up:
                    speed = bullet.GetPower().y * (-1);
                    break;
                case ShieldPosition.down:
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
