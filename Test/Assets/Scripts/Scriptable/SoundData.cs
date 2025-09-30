using UnityEditor;
using UnityEngine;

namespace Scripts.Scriptable
{
    [CreateAssetMenu(fileName = "SoundData.asset", menuName = "ScriptableObject/SoundData",order = 0)]
    public class SoundData : ScriptableObject
    {
        [Header("<プレイヤーSE>")]
        [JapaneseLabel("反射音")] public AudioClip ReflectionSound;
        [JapaneseLabel("射撃音")] public AudioClip ShotSound;
        [JapaneseLabel("被ダメ時音")] public AudioClip DamageSound;
        [JapaneseLabel("ジャンプ")] public AudioClip JumpSound;
        [JapaneseLabel("歩き")] public AudioClip WalkSound;
        [Space(5)][Header("敵SE")]
        [JapaneseLabel("撃破")] public AudioClip EnemyDestorySound;
        [JapaneseLabel("敵射撃")] public AudioClip EnemyShotSound;
        [JapaneseLabel("盾")] public AudioClip ShieldSound;
        [JapaneseLabel("ワープ")] public AudioClip WarpSound;

        
    }
}