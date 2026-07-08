using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.IntItem + nameof(IntItemsSO))]
public class IntItemsSO : DraftUtils.SerializableDictionarySO<ItemType, int>
{
}