using UnityEngine;

/// <summary>
/// Hiển thị thông tin phần thưởng là một vật phẩm đơn lẻ.
/// </summary>
public class SingleItemReward : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private RewardDataView rewardDataView;

    /// <summary>
    /// Thiết lập thông tin vật phẩm và số lượng hiển thị.
    /// </summary>
    /// <param name="reward">Thông tin phần thưởng.</param>
    public void SetData(RewardData reward)
    {
        if (rewardDataView != null)
        {
            rewardDataView.SetDataWithhPrefix(reward);
        }
    }
}
