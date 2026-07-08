using System;

namespace DraftUtils.IAP
{
    /// <summary>
    /// Loại product IAP.
    /// </summary>
    public enum IAPProductType
    {
        /// <summary>Mua 1 lần, giữ vĩnh viễn (vd: Remove Ads, Unlock Level).</summary>
        NonConsumable,

        /// <summary>Mua nhiều lần (vd: Gems, Coins).</summary>
        Consumable,

        /// <summary>Subscription (vd: VIP Monthly).</summary>
        Subscription
    }

    /// <summary>
    /// Thông tin 1 IAP product — dùng để đăng ký khi khởi tạo.
    /// </summary>
    [Serializable]
    public class IAPProductInfo
    {
        /// <summary>
        /// ID product trên store (vd: "com.mygame.gems_100").
        /// Phải khớp với App Store Connect / Google Play Console.
        /// </summary>
        public string ProductId;

        /// <summary>Loại product.</summary>
        public IAPProductType ProductType;

        /// <summary>Tên hiển thị (cho debug/UI fallback).</summary>
        public string DisplayName;

        /// <summary>Giá localized (được fill sau khi init thành công).</summary>
        public string LocalizedPrice;

        public IAPProductInfo() { }

        public IAPProductInfo(string productId, IAPProductType type, string displayName = "")
        {
            ProductId = productId;
            ProductType = type;
            DisplayName = string.IsNullOrEmpty(displayName) ? productId : displayName;
        }
    }
}
