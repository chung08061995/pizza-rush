using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.StringItem + nameof(StringItemSO))]
public class StringItemSO : DraftUtils.KeyValueEntrySO<ItemType, string>
{
}
