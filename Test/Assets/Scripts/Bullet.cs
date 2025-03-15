using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private UnityEngine.Vector3 Power;
    Player player => Player.Instance;
    public float PowerDirection;
    private int count = 1;
    private bool isAttack = false;
    public int Damage = 1;
    private Material material;
    private bool destroyed = false;//Destroyで消してもAttckに反応することがあるので仮で配置、バグ治せれば消す
    //private MeshRenderer meshRenderer;
    [SerializeField] private MeshRenderer meshRendererChild;
    private int reflectionCount;
    private int maxReflectionCount = 4;

    private UnityEngine.Vector3 SavePower;
    private void Start()
    {
        Power *= PowerDirection;
        //meshRenderer = GetComponent<MeshRenderer>();
        //meshRendererChild = GetComponentInChildren<MeshRenderer>();
        reflectionCount = 0;
    }
    void Update()
    {
        transform.position += Power*Time.deltaTime;

        if (!GetComponent<Renderer>().isVisible)
        {
            if(count >= 1)
            {
                count--;
                return;
            }
            UnityEngine.Vector3 pos = transform.position;
            if (pos.x < 0)
                transform.position = new UnityEngine.Vector3(7f, pos.y, pos.z);
            else
                transform.position = new UnityEngine.Vector3(-7f, pos.y, pos.z);
        }

        if(player.FinishAttack&&isAttack)
        {
            Attack();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground" || (collision.gameObject.tag == "Player" && !isAttack))
        {
            if (collision.TryGetComponent<PlayerStatus>(out PlayerStatus status))
            {
                status.Damage(1);
            }
            Time.timeScale = 1f;
            player.isMove = true;
            isAttack = false;
            destroyed = true;
            Destroy(this.gameObject);
        }

        if (collision.gameObject.tag == "Attack" && !destroyed)
        {
            player.isMove = false;
            player.Arrow.SetActive(true);
            player.isAttack = true;
            isAttack = true;
            Time.timeScale = 0.2f;
            Power = UnityEngine.Vector3.zero;
        }

        if (collision.gameObject.tag == "QuickAttack" && !destroyed)
        {
            player.isMove = false;
            isAttack = true;
            Time.timeScale = 0.2f;
            SavePower = -Power;
            Power = UnityEngine.Vector3.zero;
            if (player.ReflectionTime > 0)
                player.ReflectionTime -=0.5f;
            
            Invoke("QuickAttack", 0.01f);
        }
    }

    private void Attack()
    {
        Damage*=2;
        //powerlevelの変更をここに入れたい
        reflectionCount++;
        float powerColor = reflectionCount * 0.26f;
        if (reflectionCount >= maxReflectionCount)
            powerColor = 1.0f;
        //meshRenderer.material.SetFloat("_PowerLevel", powerColor);
        meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
        player.BulletTime -= 2.5f;
        player.Arrow.SetActive(false);
        player.isMove = true;
        Invoke("AttckFalse", 0.2f);
      
        if (PowerDirection < 0)
            PowerDirection *= -1;

        if (PowerDirection <= 2.0f)
            PowerDirection *= 1.25f;

        float Angle = Mathf.Atan2(player.InputMove.y, player.InputMove.x);
        UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0);
        Power = direction * PowerDirection * 10f;
        Time.timeScale =1f;
        player.FinishAttack = false;
        player.PlayEffect();
        player.PlayReflectionSound();
    }

    private void QuickAttack()
    {
        Damage *= 2;
        //powerlevelの変更をここに入れたい
        reflectionCount++;
        float powerColor = reflectionCount * 0.26f;
        if (reflectionCount >= maxReflectionCount)
            powerColor = 1.0f;
        //meshRenderer.material.SetFloat("_PowerLevel", powerColor);
        meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
        player.BulletTime -= 2.5f;
        player.isMove = true;
        Invoke("AttckFalse", 0.2f);
    
        if (PowerDirection < 0)
            PowerDirection *= -1;

        if (PowerDirection < 2.0f)
            PowerDirection *= 1.25f;

        Power = SavePower * PowerDirection;
        Time.timeScale = 1f;
        player.PlayEffect();
        player.PlayReflectionSound();
    }

    void AttckFalse()
    {
        isAttack=false;
    }
}
