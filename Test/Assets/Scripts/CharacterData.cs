using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObject/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private int initialHp;
    
    public int InitialHp{get{return initialHp;}}
}
