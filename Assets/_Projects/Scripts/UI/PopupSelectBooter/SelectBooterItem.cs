using UnityEngine;

public class SelectBooterItem : MonoBehaviour
{
    [SerializeField] private ItemView itemView;
    [SerializeField] private DraftUtils.OptionalGameObjectGroup selectObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup deselectObject = new();

    public ItemView ItemView => itemView;
    private ItemType _data;

    public void SetData(ItemType data)
    {
        _data = data;
        itemView.SetData(data);
        Deselect();
    }

    public void Select()
    {
        selectObject.SetActive(true);
        deselectObject.SetActive(false);
    }

    public void Deselect()
    {
        selectObject.SetActive(false);
        deselectObject.SetActive(true);
    }

    public ItemType GetData()
    {
        return _data;
    }
}
