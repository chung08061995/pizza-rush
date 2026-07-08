using System;

namespace DraftUtils.IAP
{
    /// <summary>
    /// Interface trung tâm cho IAP service.
    /// Dùng cùng ServiceLocator để inject implementation thật hoặc stub.
    /// 
    /// Cách dùng:
    /// <code>
    /// var iap = ServiceLocator.Get&lt;IIAPService&gt;();
    /// iap.PurchaseProduct("com.game.remove_ads", OnPurchaseComplete);
    /// </code>
    /// </summary>
    public interface IIAPService
    {
        /// <summary>IAP đã khởi tạo thành công chưa.</summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Khởi tạo IAP system với danh sách product.
        /// </summary>
        /// <param name="products">Danh sách product cần đăng ký</param>
        /// <param name="onComplete">Callback khi init xong (true = thành công)</param>
        void Initialize(IAPProductInfo[] products, Action<bool> onComplete = null);

        /// <summary>
        /// Mua product theo ID.
        /// </summary>
        /// <param name="productId">ID product (vd: "com.game.gems_100")</param>
        /// <param name="onResult">Callback kết quả mua</param>
        void PurchaseProduct(string productId, Action<IAPPurchaseResult> onResult = null);

        /// <summary>
        /// Restore các purchase đã mua (iOS bắt buộc, Android tự động).
        /// </summary>
        /// <param name="onComplete">Callback khi restore xong (true = thành công)</param>
        void RestorePurchases(Action<bool> onComplete = null);

        /// <summary>
        /// Kiểm tra product đã được mua chưa (non-consumable / subscription).
        /// </summary>
        /// <param name="productId">ID product</param>
        /// <returns>true nếu đã sở hữu</returns>
        bool IsProductOwned(string productId);

        /// <summary>
        /// Lấy giá hiển thị đã localize của product.
        /// </summary>
        /// <param name="productId">ID product</param>
        /// <returns>Chuỗi giá (vd: "$0.99", "22.000₫")</returns>
        string GetLocalizedPrice(string productId);

        /// <summary>
        /// Lấy thông tin product.
        /// </summary>
        /// <param name="productId">ID product</param>
        /// <returns>Thông tin product hoặc null nếu không tìm thấy</returns>
        IAPProductInfo GetProductInfo(string productId);
    }
}
