using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObject/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private int initialHp;
    [SerializeField] private float speed;
    
    public int InitialHp{get{return initialHp;}}
    public float Speed{get{return speed;}}
}
