using Sirenix.OdinInspector;
using UnityEngine;

public class MultipleIAPDataView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private ItemView itemView;
    [SerializeField] private DraftUtils.OptionalTMPTextGroup priceText = new();
    [SerializeField] private DraftUtils.OptionalButtonGroup button = new();
    [SerializeField] private DraftUtils.OptionalValue<DraftUtils.Pooler<RewardDataView>> economyRewardPooler = new();
    [SerializeField] private DraftUtils.OptionalValue<DraftUtils.Pooler<RewardDataView>> skillRewardPooler = new();
    [SerializeField] private DraftUtils.OptionalValue<DraftUtils.Pooler<ItemView>> featurePooler = new();
    [SerializeField] private DraftUtils.OptionalValue<RewardDataView> gold = new();

    [ShowInInspector] [ReadOnly] private MultipleIAPData _data;
    public MultipleIAPData Data => _data;

    public DraftUtils.OptionalButtonGroup Button => button;

    private void Start()
    {
        button.RegisterClickEvents();
    }

    public void SetData(MultipleIAPData data)
    {
        _data = data;
        itemView.SetData(_data.itemType);
        SetPriceText();
        SetFeatures();
        SetEconomyRewards();
        SetSkillRewards();
        SetGold();
    }
    private void SetGold()
    {
        if (!gold.isPresent)
        {
            return;
        }
        if(_data.gold == null)
        {
            return;
        }
        gold.value.SetData(_data.gold);
    }

    private void SetPriceText()
    {
        priceText.SetText(DraftUtils.IAP.IAPManager.Instance.GetDisplayPrice(
            _data.productId,
            () => DataManager.Instance.iapData.GetFallbackDisplayPrice(_data.productId)));
    }


    private void SetEconomyRewards()
    {
        if (!economyRewardPooler.isPresent) return;

        var pooler = economyRewardPooler.value;
        pooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<RewardDataView>();
        pooler.DespawnAll();

        foreach (var reward in _data.economyRewards)
        {
            var view = pooler.Spawn();
            view.SetDataWithhPrefix(reward);
        }
    }

    private void SetSkillRewards()
    {
        if (!skillRewardPooler.isPresent) return;

        var pooler = skillRewardPooler.value;
        pooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<RewardDataView>();
        pooler.DespawnAll();

        foreach (var reward in _data.skillRewards)
        {
            var view = pooler.Spawn();
            view.SetDataWithhPrefix(reward);
        }
    }

    private void SetFeatures()
    {
        if (!featurePooler.isPresent) return;

        var pooler = featurePooler.value;
        pooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<ItemView>();
        pooler.DespawnAll();

        foreach (var featureType in _data.features)
        {
            var view = pooler.Spawn();
            view.SetData(featureType);
        }
    }
}
