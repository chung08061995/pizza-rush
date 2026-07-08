using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý việc hiển thị danh sách nhiều vật phẩm phần thưởng khác nhau.
/// </summary>
public class MultipleItemReward : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.Pooler<RewardDataView> rewardPooler = new();

    public System.Collections.Generic.IReadOnlyList<RewardDataView> ActiveItems => rewardPooler.ActiveItems;

    /// <summary>
    /// Tạo danh sách các phần thưởng con sử dụng Pooler.
    /// </summary>
    /// <param name="rewards">Danh sách các phần thưởng RewardData.</param>
    public void SetData(List<RewardData> rewards)
    {
        if (rewardPooler == null)
        {
            return;
        }

        // Khởi tạo Factory cho Pooler và thu hồi toàn bộ item đang hoạt động
        rewardPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<RewardDataView>();
        rewardPooler.DespawnAll();

        if (rewards == null)
        {
            return;
        }

        // Spawn các item hiển thị từ pool và cập nhật dữ liệu
        foreach (var reward in rewards)
        {
            var view = rewardPooler.Spawn();
            view.SetDataWithhPrefix(reward);
        }
    }
}
