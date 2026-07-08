using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BackgroundDataCreator : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public SpriteItemsSO previewIconSO;
    [SerializeField] public SpriteItemsSO iconSO;
    [SerializeField] public StringItemsSO descriptionSO;

    [ShowInInspector][ReadOnly] public Dictionary<ItemType, BackgroundData> backgrounds = new();

    public void Initialized()
    {
        backgrounds.Clear();
        previewIconSO.BuildDictionary();
        iconSO.BuildDictionary();
        descriptionSO.BuildDictionary();
        
        foreach (var itemType in previewIconSO.Dictionary.Keys)
        {
            BackgroundData background = new();
            background.itemType = itemType;
            backgrounds.Add(itemType, background);
        }

        foreach (var itemType in previewIconSO.Dictionary.Keys)
        {
            var background = backgrounds[itemType];
            background.previewIcon = previewIconSO.Dictionary[itemType];
            background.icon = iconSO.Dictionary[itemType];
            background.description = descriptionSO.Dictionary[itemType];
        }
    }
}
