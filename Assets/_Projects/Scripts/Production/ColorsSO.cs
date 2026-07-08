using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.Color + nameof(ColorsSO))]
public class ColorsSO : DraftUtils.SerializableDictionarySO<ColorType, Color>
{

}
