using Scripts.Scriptable;
using UnityEngine;

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
        left = 0,
        right = 1,
        up = 2,
        down = 3,
    }

    public bool CanLoop(Vector3 pos, Side side)
    {
        //Debug.Log(pos);
        if (checkSkip == true)
        {
            return true;
        }
        
        if (side == Side.up && mapData.UpDownCantArea != null)
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
        else if (side == Side.down && mapData.DownUpCantArea != null)
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
