using UnityEngine;


[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.ColorMaterial + nameof(ColorItemSO))]
public class ColorItemSO : DraftUtils.KeyValueEntrySO<ColorType, Material>
{
}
