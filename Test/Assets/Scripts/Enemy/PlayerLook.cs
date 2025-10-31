using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerLook : MonoBehaviour
{
    [JapaneseLabel("プレイヤー")] public Player.Player player => Player.Player.Instance;

    // [SerializeField] GameObject playerLookObj;
    private float speed = 10f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // transform.LookAt(player.transform);
        
        // Vector3 dir = player.transform.position - transform.position;
        // Quaternion rotation = Quaternion.LookRotation(dir);
        // transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (player != null)
        {
            Debug.Log("EnemyAIOnAnimatorIK");
            //transform.DOLookAt(player.transform.localPosition, 0.5f);
            //playerLookObj.transform.DOLookAt(player.transform.localPosition, 0.5f);
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.LookAt(player.transform);
            // DOLookAt(ターゲットの位置, 回転にかける時間)
            //transform.DOLookAt(player.transform.localPosition, 0.5f);
            
            // Vector3 dir = player.transform.position - transform.position;
            // Quaternion rotation = Quaternion.LookRotation(dir);
            // transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);
        }
    }
}
