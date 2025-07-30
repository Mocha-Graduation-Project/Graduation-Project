using UnityEngine;
namespace Scripts
{
    public class EffectActive : MonoBehaviour
    {
        [SerializeField] private float effectTime = 0.5f;
        private float time;
        bool active = false;

        private void Update()
        {
            if (!active) return;
            
            time += Time.fixedDeltaTime;
            if (effectTime <= time)
            {
                time = 0;
                ActiveFalse();

            }

        }
        private void OnEnable()
        {
            time = 0;
            active = true;
        }

        private void ActiveFalse()
        {
            this.gameObject.SetActive(false);
        }
    }
}