
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMoreLives : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private ItemView goldView;
    [SerializeField] private DraftUtils.OptionalButtonGroup watchAdsButton;
    [SerializeField] private ItemView liveItem;
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] public HeartRecoveryView heartRecoveryView;

    [SerializeField] private DraftUtils.OptionalButtonGroup buyButton;



    public DraftUtils.OptionalButtonGroup WatchAdsButton => watchAdsButton;
    public DraftUtils.OptionalButtonGroup BuyButton => buyButton;
    private void Start()
    {

        buyButton.RegisterClickEvents();
        watchAdsButton.RegisterClickEvents();

        goldView.RemaningText.ValueToDisplayTextFunc = x => DraftUtils.Utils.Common.FormatNumber((int)x);
        goldView.SetData(ItemType.Gold);

    }
    public void SetData()
    {
        heartRecoveryView.SetData(DataManager.Instance.heartRecoveryState.Value);
        liveItem.SetData(ItemType.Booter_LifeTime);
    }
}
