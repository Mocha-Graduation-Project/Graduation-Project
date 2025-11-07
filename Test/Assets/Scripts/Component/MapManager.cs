using Scripts.Scriptable;
using UnityEngine;

namespace Component
{
    enum GameplayState
    {
        Title,
        Normal,
        MoveBoss,
        Depth,
        GameOver,
        GameClear
    }
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private MapData mapData;
        [SerializeField] private bool checkSkip;
        [SerializeField] private SoundData soundData;
        
        [SerializeField] private GameplayState gameplayState = GameplayState.Normal;
        private AudioSource audioSource;
        private AudioClip bgm;
        
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            switch (gameplayState)
            {
                case GameplayState.Title:
                  bgm = soundData.Title;
                    break;
                case GameplayState.Normal:
                    bgm = soundData.Normal;
                    break;
                case GameplayState.MoveBoss:
                    bgm = soundData.MoveBoss;
                    break;
                case GameplayState.Depth:
                    bgm = soundData.Depth;
                    break;
                case GameplayState.GameOver:
                    bgm = soundData.GameOver;
                    break;
                case GameplayState.GameClear:
                    bgm = soundData.GameClear;
                    break;
            }

            audioSource.clip = bgm;
            audioSource.Play();
        }
        public enum Side
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3,
        }

        public void Clear()
        {
            audioSource.Stop();
            bgm = soundData.GameClear;
            audioSource.clip = bgm; 
            audioSource.Play();
        }

        public void GameOver()
        {
            audioSource.Stop();
            bgm = soundData.GameOver;
            audioSource.clip = bgm; 
            audioSource.Play();
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
