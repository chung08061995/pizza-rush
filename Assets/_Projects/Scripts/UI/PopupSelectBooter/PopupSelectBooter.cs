using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupSelectBooter : MonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private SelectBooterItem coffeeItemView;
    [SerializeField] private SelectBooterItem magicItemView;
    [SerializeField] private Button playButton;
    private List<ItemType> _booterItems = new();
    private Action _onPlay;

    private void Start()
    {
        popup.closeButton.OnClickAction = popup.HideWithAnimation;

        playButton.onClick.AddListener(ClickPlay);
        coffeeItemView.SetData(ItemType.Booter_CoffeeTime);
        magicItemView.SetData(ItemType.Booter_Magic);

        coffeeItemView.ItemView.Button.OnClickAction = ClickCoffeeButton;
        magicItemView.ItemView.Button.OnClickAction = ClickMagicButton;

    }

    private void ClickMagicButton()
    {
        SelectItem(magicItemView);
    }

    private void ClickCoffeeButton()
    {
        SelectItem(coffeeItemView);
    }

    private void ClickPlay()
    {
        RuntimeStorage.Instance.Set(GameConstain.RuntimeStorage.StartBooterItems, _booterItems);
        popup.HideWithAnimation();
        _onPlay?.Invoke();
    }

    private void SelectItem(SelectBooterItem item)
    {
        Debug.LogError(item.GetData().ToString());
        if (item == null)
        {
            return;
        }

        if (GetRemainingFromDataManager(item.GetData()) <= 0)
        {
            return;
        }

        if (_booterItems.Contains(item.GetData()))
        {
            _booterItems.Remove(item.GetData());
            item.Deselect();
        }
        else
        {
            _booterItems.Add(item.GetData());
            item.Select();
        }
    }

    private int GetRemainingFromDataManager(ItemType itemType)
    {
        if(DataManager.Instance.remainningItems.TryGetValue(itemType, out var data))
        {
            return data.Value;
        }
        return 0;
    }

    public void SetData(Action onPlay = null)
    {
        _onPlay = onPlay;
        _booterItems.Clear();
        coffeeItemView.SetData(ItemType.Booter_CoffeeTime);
        magicItemView.SetData(ItemType.Booter_Magic);
    }
}
