using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.SpriteItem + nameof(SpriteItemsSO))]
public class SpriteItemsSO : DraftUtils.SerializableDictionarySO<ItemType, Sprite>
{
}