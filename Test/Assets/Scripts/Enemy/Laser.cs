using System;
using Player;
using Scripts;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("LaserHit");
            if (other.TryGetComponent<PlayerStatus>(out PlayerStatus status))
            {
                status.Damage(1);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("LaserHit");
            if (other.TryGetComponent<PlayerStatus>(out PlayerStatus status))
            {
                status.Damage(1);
            }
        }
    }
}
