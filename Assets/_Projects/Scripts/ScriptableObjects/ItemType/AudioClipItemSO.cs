using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.AudioClipItem + nameof(AudioClipItemSO))]
public class AudioClipItemSO : DraftUtils.KeyValueEntrySO<ItemType, AudioClip>
{
}
