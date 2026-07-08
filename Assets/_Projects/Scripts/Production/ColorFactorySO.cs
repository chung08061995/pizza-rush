using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.ColorMaterial + nameof(ColorFactorySO))]
public class ColorFactorySO : DraftUtils.SerializableDictionarySO<ColorType, Material>
{
}
