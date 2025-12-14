using UnityEngine;
using UnityEngine.UI;

namespace Systems
{
    [DefaultExecutionOrder(-10)]
    public class EnemyHPSlider  : MonoBehaviour
    {
        public static EnemyHPSlider Instance { get; private set; }
        [SerializeField] private Slider  slider;
        public Slider Slider => slider;

        private void Awake()
        {
            // インスタンスを登録
            if (Instance == null)
            {
                Instance = this;
                if (slider == null) slider = GetComponent<Slider>();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}