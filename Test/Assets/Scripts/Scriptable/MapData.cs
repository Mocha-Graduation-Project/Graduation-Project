using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/MapData")]
public class MapData : ScriptableObject
{
    [SerializeField] private float[] upDownCantArea;//上から下に移動できないエリア(下側のオブジェクト)
    [SerializeField] private float[] downUpCantArea;//下から上に移動できないエリア(上側のオブジェクト)

    public float[] UpDownCantArea{get{return upDownCantArea;}}
    public float[] DownUpCantArea{get{return downUpCantArea;}}
}
