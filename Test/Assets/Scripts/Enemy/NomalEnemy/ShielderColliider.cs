using System;
using UnityEngine;

public class ShielderColliider : MonoBehaviour
{
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

        //Debug.Log("本体当たった:" + other.gameObject.name);
        
        if (other.CompareTag("Bullet"))
        {
            Player.Bullet bullet = other.gameObject.GetComponent<Player.Bullet>();
            shielder.TakeDamage(bullet.Damage);
        }
    }
}
