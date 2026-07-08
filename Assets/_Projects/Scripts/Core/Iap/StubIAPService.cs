using System;
using UnityEngine;

namespace DraftUtils.IAP
{
    /// <summary>
    /// Stub IAP service — dùng khi CHƯA cài Unity IAP package.
    /// Mọi hàm đều log warning và trả kết quả mặc định an toàn.
    /// 
    /// Đảm bảo game không bị crash khi:
    /// - Chưa cài com.unity.purchasing
    /// - Build test không cần IAP
    /// - Editor development
    /// 
    /// Tự động được dùng khi không define UNITY_IAP.
    /// </summary>
    public class StubIAPService : IIAPService
    {
        private const string TAG = "[IAP-Stub]";

        public bool IsInitialized => false;

        public void Initialize(IAPProductInfo[] products, Action<bool> onComplete = null)
        {
            Debug.LogWarning($"{TAG} IAP chưa được cài đặt. Cài package 'com.unity.purchasing' " +
                             $"và thêm define 'UNITY_IAP' để kích hoạt.");
            onComplete?.Invoke(false);
        }

        public void PurchaseProduct(string productId, Action<IAPPurchaseResult> onResult = null)
        {
            Debug.LogWarning($"{TAG} Không thể mua '{productId}' — IAP chưa được cài đặt.");
            onResult?.Invoke(IAPPurchaseResult.Failure(
                productId, IAPFailureReason.NotInitialized, "Unity IAP package not installed."));
        }

        public void RestorePurchases(Action<bool> onComplete = null)
        {
            Debug.LogWarning($"{TAG} Không thể restore — IAP chưa được cài đặt.");
            onComplete?.Invoke(false);
        }

        public bool IsProductOwned(string productId)
        {
            return false;
        }

        public string GetLocalizedPrice(string productId)
        {
            return "N/A";
        }

        public IAPProductInfo GetProductInfo(string productId)
        {
            return null;
        }
    }
}
