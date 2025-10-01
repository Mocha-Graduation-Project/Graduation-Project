using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private GameObject arrowPos;

        private void Update()
        {
            transform.position = arrowPos.transform.position;
        }
    }
}
