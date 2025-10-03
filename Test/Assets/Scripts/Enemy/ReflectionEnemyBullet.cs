using Component;
using Scripts;
using UnityEngine;

public class ReflectionEnemyBullet : MonoBehaviour
{
    [SerializeField] Material material;
    Material childMaterial;
    [SerializeField] private GameObject childObj;
    [SerializeField] private Loop loop;
    [SerializeField] private bool loopAble;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        childMaterial = childObj.GetComponent<Renderer>().material;
        if (loopAble == false)
        {
            Invoke("Destroy", 5);
        }
        //Debug.Log(childMaterial);
    }

    public void ChangeMaterial()
    {
        childObj.GetComponent<Renderer>().material = material;
        Debug.Log(childMaterial);

        if (loop != null)
        {
            loop.enabled = true;
        }
        if (loopAble == false)
        {
            CancelInvoke("Destroy");
        }
    }

    void Destroy()
    {
        Destroy(this.gameObject);
    }
}
