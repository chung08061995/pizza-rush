using System;
using UnityEngine;

public class AvatarDataListenerView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private ItemView itemView;
    public ItemView ItemView => itemView;

    private DraftUtils.PersistentValue<ItemType> _currentAvatar => DataManager.Instance.currentAvatar;
    void Start()
    {
        _currentAvatar.Notifier.AddListener(CurrentAvatarChanged);
        CurrentAvatarChanged();

    }
    private void OnDestroy()
    {
        _currentAvatar.Notifier.RemoveListener(CurrentAvatarChanged);
    }

    private void CurrentAvatarChanged()
    {
        itemView.SetData(_currentAvatar.Value);
    }
}
