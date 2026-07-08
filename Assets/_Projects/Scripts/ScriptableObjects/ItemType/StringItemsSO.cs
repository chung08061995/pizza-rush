using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.StringItem + nameof(StringItemsSO))]
public class StringItemsSO : DraftUtils.SerializableDictionarySO<ItemType, string>
{
}
