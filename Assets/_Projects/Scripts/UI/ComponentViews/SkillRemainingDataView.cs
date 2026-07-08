
using Sirenix.OdinInspector;
using UnityEngine;

public class SkillRemainingDataView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalButtonGroup addMoreButton;
    [SerializeField] private DraftUtils.OptionalButtonGroup button;
    [SerializeField] private ItemView itemView;
    [SerializeField] private LockContentComponent lockContent;
    private ItemType _data;



    public DraftUtils.OptionalButtonGroup Button => button;

    private void Start()
    {
        button.RegisterClickEvents();
        addMoreButton.RegisterClickEvents();
    }
    public void SetData(ItemType data)
    {
        _data = data;
        SetItemView();
        SetLockContent();
    }
    [Button]
    public void Reload()
    {
        SetData(_data);
    }

    private void SetItemView()
    {
        itemView.SetData(_data);
        itemView.SetRemaningTextActive();
    }

    private void SetLockContent()
    {
        if (DataManager.Instance.levelUnlockItems.TryGetValue(_data, out var unlockLevel))
        {
            lockContent.SetData(unlockLevel);
        }
    }
}