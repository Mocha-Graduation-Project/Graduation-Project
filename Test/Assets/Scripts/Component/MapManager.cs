using Scripts.Scriptable;
using UnityEngine;

namespace Component
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private MapData mapData;
        [SerializeField] private bool checkSkip;
        [SerializeField] private SoundData soundData;
    
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
        }
        public enum Side
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3,
        }

        public bool CanLoop(Vector3 pos, Side side)
        {
            //Debug.Log(pos);
            if (checkSkip == true)
            {
                return true;
            }
        
            if (side == Side.Up && mapData.UpDownCantArea != null)
            {
                for (int i = 0; i < mapData.UpDownCantArea.Length; i += 2)
                {
                    //オブジェクトの左側よりも大きい且つ右側よりも小さい(オブジェクトの内部)の場合ループできない
                    if (pos.x > mapData.UpDownCantArea[i] && pos.x < mapData.UpDownCantArea[i + 1])
                    {
                        return false;
                    }
                }
            }
            else if (side == Side.Down && mapData.DownUpCantArea != null)
            {
                for (int i = 0; i < mapData.DownUpCantArea.Length; i += 2)
                {
                    //オブジェクトの左側よりも大きい且つ右側よりも小さい(オブジェクトの内部)の場合ループできない
                    if (pos.x > mapData.DownUpCantArea[i] && pos.x < mapData.DownUpCantArea[i + 1])
                    {
                        return false;
                    }
                }
            }
        
            return true;
        }
    }
}
