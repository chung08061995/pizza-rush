using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.SpriteItem + nameof(SpriteItemSO))]
public class SpriteItemSO : DraftUtils.KeyValueEntrySO<ItemType, Sprite>
{
}