using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILife : MonoBehaviour
{
    [SerializeField] private GameObject lifePrefab;
    [SerializeField] private List<GameObject> lifes;
    private Vector3 initialpos;
    private int lifeCount;
    private const int SIZE = 60;
    private const float SCALE = 0.5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("UILife Start");
        // lifes=new List<GameObject>();
        // initialpos = new Vector3(-360, 190, 0);
        // lifeCount = 0;
    }

    void Awake()
    {
        Debug.Log("UILife Awake");
        lifes=new List<GameObject>();
        initialpos = new Vector3(-360, 190, 0);
        lifeCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLife()
    {
        GameObject newLife = Instantiate(lifePrefab);
        newLife.transform.SetParent(this.transform);
        Vector3 localPos = initialpos;
        localPos.x += SIZE * lifeCount;
        // Debug.Log(localPos.x + ":" + SIZE + ":" + lifeCount);
        newLife.transform.localPosition = localPos;
        newLife.transform.localScale = new Vector3(SCALE, SCALE, SCALE);
        lifes.Add(newLife);
        lifeCount++;
    }

    public void RemoveLife()
    {
        lifeCount--;
        if (lifeCount >= 0)
        {
            GameObject lifeToRemove = lifes[lifeCount];
            lifes.RemoveAt(lifeCount);
            Destroy(lifeToRemove);
        }
    }
}
