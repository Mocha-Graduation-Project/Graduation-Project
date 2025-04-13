using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
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
    CameraAreaManager cameraAreaManager;

    private UnityEngine.Vector3 SavePower;
    
    [SerializeField] private float reflectionCooldown = 1.0f; // 反射できるようになるまでの秒数
    private float spawnTime; // 発射された時間
    private void Start()
    {
        Power *= PowerDirection;
        //meshRenderer = GetComponent<MeshRenderer>();
        //meshRendererChild = GetComponentInChildren<MeshRenderer>();
        Debug.Log(meshRendererChild.name);
        reflectionCount = 0;
        cameraAreaManager = GameObject.FindObjectOfType<CameraAreaManager>();
        spawnTime = Time.time; // 現在の時間を記録
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
                
            if (pos.x < cameraAreaManager.LeftMax)
                pos.x = cameraAreaManager.RightMax;
            else if(pos.x > cameraAreaManager.RightMax)
                pos.x = cameraAreaManager.LeftMax;

            if (pos.y < cameraAreaManager.DownMax)
                pos.y = cameraAreaManager.UpMax;
            else if (pos.y > cameraAreaManager.UpMax)
                pos.y = cameraAreaManager.DownMax;

            transform.position = pos;
            spawnTime = 1;
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

        if (collision.gameObject.tag == "Attack" && !destroyed && Time.time - spawnTime >= reflectionCooldown)
        {
            player.isMove = false;
            player.Arrow.SetActive(true);
            isAttack = true;
            SavePower = -Power;
            Power = UnityEngine.Vector3.zero;
            Invoke("Attack", 0.1f);
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
        
        PowerDirection *= 1.25f;
        if (PowerDirection < 0)
            PowerDirection *= -1;
        
        float Angle = Mathf.Atan2(player.InputMove.y, player.InputMove.x);
        UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0);
        Power = direction * PowerDirection * 10f;
        
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
        PowerDirection *= 1.25f;
        if (PowerDirection < 0)
            PowerDirection *= -1;
        
        Power = SavePower * PowerDirection;
        Time.timeScale = 1f;
        player.PlayReflectionSound();
    }
    
    public Vector3 GetPower()
    {
        return Power;
    }

    public void SetPower(Vector3 newPower)
    {
        Power = newPower;
    }
    
    public void OnReflect()
    {
        player.PlayReflectionSound();
    }

    void AttckFalse()
    {
        isAttack=false;
    }
}
