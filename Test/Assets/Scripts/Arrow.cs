using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private GameObject arrowpos;
    private void Update()
    {
        transform.position = arrowpos.transform.position;
    }
}
