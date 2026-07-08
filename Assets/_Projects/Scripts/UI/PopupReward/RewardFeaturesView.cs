using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// View phụ trợ quản lý hoặc chứa các thành phần chức năng (features) hiển thị trong popup phần thưởng.
/// </summary>
public class RewardFeaturesView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.Pooler<ItemView> featurePooler = new();

    public System.Collections.Generic.IReadOnlyList<ItemView> ActiveItems => featurePooler.ActiveItems;

    /// <summary>
    /// Hiển thị danh sách các tính năng (features) sử dụng Pooler của ItemView.
    /// </summary>
    /// <param name="features">Danh sách tính năng cần hiển thị.</param>
    public void SetData(List<ItemType> features)
    {
        if (featurePooler == null)
        {
            return;
        }

        featurePooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<ItemView>();
        featurePooler.DespawnAll();

        if (features == null)
        {
            return;
        }

        foreach (var feature in features)
        {
            var view = featurePooler.Spawn();
            view.SetData(feature);
        }
    }
}
