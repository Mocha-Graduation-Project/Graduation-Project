using UnityEngine;

namespace Component
{
    public class BackRotation : MonoBehaviour
    {
        private void Update()
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}