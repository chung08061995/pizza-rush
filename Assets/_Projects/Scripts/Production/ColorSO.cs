using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.Color + nameof(ColorSO))]

public class ColorSO : DraftUtils.KeyValueEntrySO<ColorType, Color>
{
}
