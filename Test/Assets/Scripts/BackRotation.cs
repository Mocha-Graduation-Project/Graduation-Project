using System;
using UnityEngine;
using System.Collections;

namespace Scripts
{
    public class BackRotation : MonoBehaviour
    {
        private Vector3 def;

        void Awake()
        {
            def = transform.localRotation.eulerAngles;
            
        }

        private void Update()
        {
            Vector3 parent = transform.parent.transform.localRotation.eulerAngles;
            
            transform.localRotation = Quaternion.Euler(def - parent);
        }
    }
}