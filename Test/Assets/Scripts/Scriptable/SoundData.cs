using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Scriptable
{
    [CreateAssetMenu(fileName = "SoundData.asset", menuName = "ScriptableObject/SoundData",order = 0)]
    public class SoundData : ScriptableObject
    {
        [Header("<プレイヤーSE>")]
        [JapaneseLabel("反射音")] public AudioClip reflectionSound;
        [JapaneseLabel("射撃音")] public AudioClip shotSound;
        [JapaneseLabel("被ダメ時音")] public AudioClip damageSound;
        [JapaneseLabel("ジャンプ")] public AudioClip jumpSound;
        [JapaneseLabel("歩き")] public AudioClip walkSound;
        [Space(5)][Header("敵SE")]
        [JapaneseLabel("撃破")] public AudioClip enemyDestroySound;
        [JapaneseLabel("敵射撃")] public AudioClip enemyShotSound;
        [JapaneseLabel("盾")] public AudioClip shieldSound;
        [JapaneseLabel("ワープ")] public AudioClip warpSound;
        [JapaneseLabel("壁反射音")]　 public AudioClip wallSound;
        [Space(5)][Header("BGM")]
        [JapaneseLabel("Title")] public AudioClip title;
        [JapaneseLabel("Normal")] public AudioClip normal;
        [JapaneseLabel("MoveBoss")] public AudioClip moveBoss;
        [JapaneseLabel("Depth")] public AudioClip depth;
        [JapaneseLabel("GameOver")] public AudioClip gameOver;
        [JapaneseLabel("GameClear")] public AudioClip gameClear;
        
        
    }
}