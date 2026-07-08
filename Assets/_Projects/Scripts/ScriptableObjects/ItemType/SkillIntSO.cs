using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.IntItem + nameof(IntItemSO))]
public class IntItemSO : DraftUtils.KeyValueEntrySO<ItemType, int>
{
}