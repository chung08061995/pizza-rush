using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Popup nhận phần thưởng (Reward), hỗ trợ hiển thị đơn lẻ hoặc danh sách phần thưởng.
/// </summary>
public class PopupReward : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] public DraftUtils.Popup popup;
    [SerializeField] private DraftUtils.RebuildLayouts rebuilder;
    [SerializeField] private Transform contentRoot;

    [Header("Views phần thưởng")]
    [SerializeField] private SingleItemReward singleItemReward;
    [SerializeField] private MultipleItemReward multipleItemReward;
    [SerializeField] private RewardFeaturesView rewardFeaturesView;

    public SingleItemReward SingleItemReward => singleItemReward;
    public MultipleItemReward MultipleItemReward => multipleItemReward;
    public RewardFeaturesView RewardFeaturesView => rewardFeaturesView;

    private void Start()
    {
        if (popup != null && popup.closeButton != null)
        {
            popup.closeButton.OnClickAction = popup.HideWithAnimation;
        }
    }

    /// <summary>
    /// Hiển thị phần thưởng là một vật phẩm đơn lẻ.
    /// </summary>
    /// <param name="reward">Thông tin phần thưởng.</param>
    public void SetDataSingle(RewardData reward)
    {
        if (singleItemReward != null)
        {
            singleItemReward.gameObject.SetActive(true);
            singleItemReward.SetData(reward);
        }
        RebuildLayout();
    }

    /// <summary>
    /// Hiển thị danh sách nhiều phần thưởng khác nhau.
    /// </summary>
    /// <param name="rewards">Danh sách phần thưởng.</param>
    public void SetDataMultiple(List<RewardData> rewards)
    {
        if (multipleItemReward != null)
        {
            multipleItemReward.gameObject.SetActive(true);
            multipleItemReward.SetData(rewards);
        }
        RebuildLayout();
    }

    /// <summary>
    /// Hiển thị danh sách các tính năng thưởng kèm theo.
    /// </summary>
    /// <param name="features">Danh sách tính năng.</param>
    public void SetDataFeatures(List<ItemType> features)
    {
        if (rewardFeaturesView != null)
        {
            rewardFeaturesView.gameObject.SetActive(true);
            rewardFeaturesView.SetData(features);
        }
        RebuildLayout();
    }


    public void RebuildLayout()
    {
        if (rebuilder != null)
        {
            rebuilder.Rebuild();
        }
    }
}