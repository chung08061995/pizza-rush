using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AudioClipDataCreator : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public AudioClipItemsSO audioClipSO;

    [ShowInInspector][ReadOnly] public Dictionary<ItemType, AudioClipData> clips = new();

    public void Initialized()
    {
        clips.Clear();

        if (audioClipSO == null)
        {
            return;
        }

        audioClipSO.BuildDictionary();
        foreach (var itemType in audioClipSO.Dictionary.Keys)
        {
            clips[itemType] = new AudioClipData
            {
                itemType = itemType,
                Clip = audioClipSO.Dictionary[itemType]
            };
        }
    }
}
