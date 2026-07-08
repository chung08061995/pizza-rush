using Sirenix.OdinInspector;
using UnityEngine;

public class RewardDataView : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private DraftUtils.OptionalImageGroup icon = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup amountText = new();
    public DraftUtils.OptionalTMPTextGroup AmountText => amountText;

    private RewardData _data;

    public void SetData(RewardData data)
    {
        _data = data;
        SetIcon();
        SetAmountText();
    }
    [Button]
    private void Reload()
    {
        SetData(_data);
    }

    public void SetIcon()
    {
        if (DataManager.Instance.iconItemsSO.TryGetValue(_data.itemType, out var sprite))
        {
            icon.SetSprite(sprite);
        }
    }

    private void SetAmountText()
    {
        amountText.SetText(_data.amount);
    }

    public void SetDataWithhPrefix(RewardData data)
    {
        _data = data;
        SetIcon();
        SetAmountTextWithhPrefix();
    }
    public void SetAmountTextWithhPrefix()
    {
        if(_data.itemType == ItemType.Booter_LifeTime)
        {
            amountText.ValueToDisplayTextFunc = FormatAmountLifeTime;
            
        }
        else
        {
            amountText.ValueToDisplayTextFunc = FormatAmountWithPrefix;
        }
        amountText.SetText(_data.amount);
    }

    /// <summary>
    /// Format: "{amount}" — nếu itemType là Booter_LifeTime thì "{amount}m"
    /// </summary>
    public static string FormatAmount(object obj)
    {
        if (obj is not int amount) return string.Empty;
        return amount.ToString();
    }

    /// <summary>
    /// Format: "x{amount}" — nếu itemType là Booter_LifeTime thì "x{amount}m"
    /// </summary>
    public static string FormatAmountWithPrefix(object obj)
    {
        if (obj is not int amount) return string.Empty;
        return $"x{amount}";
    }

    /// <summary>
    /// Format cho Booter_LifeTime: "{amount}m"
    /// </summary>
    public static string FormatAmountLifeTime(object obj)
    {
        if (obj is not int amount) return string.Empty;
        return $"{amount}m";
    }

    /// <summary>
    /// Format cho Booter_LifeTime: "x{amount}m"
    /// </summary>
    public static string FormatAmountLifeTimeWithPrefix(object obj)
    {
        if (obj is not int amount) return string.Empty;
        return $"x{amount}m";
    }
}
