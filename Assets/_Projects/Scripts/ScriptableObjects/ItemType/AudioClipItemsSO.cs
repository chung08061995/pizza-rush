using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.AudioClipItem + nameof(AudioClipItemsSO))]
public class AudioClipItemsSO : DraftUtils.SerializableDictionarySO<ItemType, AudioClip>
{
}
