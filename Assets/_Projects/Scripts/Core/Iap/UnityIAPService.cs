#if UNITY_IAP
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace DraftUtils.IAP
{
    /// <summary>
    /// Implementation thật của IIAPService sử dụng Unity IAP (com.unity.purchasing).
    /// 
    /// File này CHỈ được compile khi có define symbol UNITY_IAP.
    /// Unity IAP package tự thêm symbol này khi cài qua Package Manager.
    /// 
    /// Cách dùng:
    /// <code>
    /// // Trong Bootstrap/Initializer:
    /// var iapService = new UnityIAPService();
    /// ServiceLocator.Register&lt;IIAPService&gt;(iapService);
    /// 
    /// var products = new[]
    /// {
    ///     new IAPProductInfo("com.game.remove_ads", IAPProductType.NonConsumable, "Remove Ads"),
    ///     new IAPProductInfo("com.game.gems_100", IAPProductType.Consumable, "100 Gems"),
    /// };
    /// iapService.Initialize(products);
    /// </code>
    /// </summary>
    public class UnityIAPService : IIAPService, IDetailedStoreListener
    {
        private const string TAG = "[IAP]";

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private Dictionary<string, IAPProductInfo> _productInfoMap = new();

        private Action<bool> _initCallback;
        private Action<IAPPurchaseResult> _purchaseCallback;
        private Action<bool> _restoreCallback;

        public bool IsInitialized => _storeController != null;

        public void Initialize(IAPProductInfo[] products, Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                Debug.Log($"{TAG} Đã khởi tạo rồi.");
                onComplete?.Invoke(true);
                return;
            }

            _initCallback = onComplete;
            _productInfoMap.Clear();

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var info in products)
            {
                var unityType = ConvertProductType(info.ProductType);
                builder.AddProduct(info.ProductId, unityType);
                _productInfoMap[info.ProductId] = info;
            }

            Debug.Log($"{TAG} Đang khởi tạo với {products.Length} products...");
            UnityPurchasing.Initialize(this, builder);
        }

        public void PurchaseProduct(string productId, Action<IAPPurchaseResult> onResult = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"{TAG} Chưa khởi tạo, không thể mua '{productId}'.");
                onResult?.Invoke(IAPPurchaseResult.Failure(
                    productId, IAPFailureReason.NotInitialized));
                return;
            }

            var product = _storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"{TAG} Product '{productId}' không khả dụng.");
                onResult?.Invoke(IAPPurchaseResult.Failure(
                    productId, IAPFailureReason.ProductUnavailable));
                return;
            }

            _purchaseCallback = onResult;
            Debug.Log($"{TAG} Bắt đầu mua '{productId}'...");
            _storeController.InitiatePurchase(product);
        }

        public void RestorePurchases(Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"{TAG} Chưa khởi tạo, không thể restore.");
                onComplete?.Invoke(false);
                return;
            }

            _restoreCallback = onComplete;

#if UNITY_IOS
            var apple = _extensionProvider.GetExtension<IAppleExtensions>();
            Debug.Log($"{TAG} Đang restore purchases (iOS)...");
            apple.RestoreTransactions((success, error) =>
            {
                Debug.Log($"{TAG} Restore iOS: {(success ? "thành công" : $"thất bại - {error}")}");
                _restoreCallback?.Invoke(success);
                _restoreCallback = null;
            });
#elif UNITY_ANDROID
            var google = _extensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            Debug.Log($"{TAG} Đang restore purchases (Android)...");
            google.RestoreTransactions((success, error) =>
            {
                Debug.Log($"{TAG} Restore Android: {(success ? "thành công" : $"thất bại - {error}")}");
                _restoreCallback?.Invoke(success);
                _restoreCallback = null;
            });
#else
            Debug.Log($"{TAG} Restore không hỗ trợ trên platform này.");
            onComplete?.Invoke(false);
#endif
        }

        public bool IsProductOwned(string productId)
        {
            if (!IsInitialized) return false;

            var product = _storeController.products.WithID(productId);
            if (product == null) return false;

            // Non-consumable hoặc Subscription: check receipt
            return product.hasReceipt;
        }

        public string GetLocalizedPrice(string productId)
        {
            if (!IsInitialized) return "N/A";

            var product = _storeController.products.WithID(productId);
            if (product == null) return "N/A";

            return product.metadata.localizedPriceString;
        }

        public IAPProductInfo GetProductInfo(string productId)
        {
            if (_productInfoMap.TryGetValue(productId, out var info))
            {
                // Cập nhật giá localized nếu đã init
                if (IsInitialized)
                {
                    var product = _storeController.products.WithID(productId);
                    if (product != null)
                    {
                        info.LocalizedPrice = product.metadata.localizedPriceString;
                    }
                }
                return info;
            }
            return null;
        }

        // ─────────────────────────────────────────────
        // IDetailedStoreListener Implementation
        // ─────────────────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log($"{TAG} Khởi tạo thành công!");
            _storeController = controller;
            _extensionProvider = extensions;

            // Cập nhật localized price cho tất cả products
            foreach (var product in controller.products.all)
            {
                if (_productInfoMap.TryGetValue(product.definition.id, out var info))
                {
                    info.LocalizedPrice = product.metadata.localizedPriceString;
                }
            }

            _initCallback?.Invoke(true);
            _initCallback = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"{TAG} Khởi tạo thất bại: {error}");
            _initCallback?.Invoke(false);
            _initCallback = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"{TAG} Khởi tạo thất bại: {error} — {message}");
            _initCallback?.Invoke(false);
            _initCallback = null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;
            var receipt = args.purchasedProduct.receipt;
            var transactionId = args.purchasedProduct.transactionID;

            Debug.Log($"{TAG} Mua thành công: '{productId}'");

            _purchaseCallback?.Invoke(IAPPurchaseResult.Success(productId, receipt, transactionId));
            _purchaseCallback = null;

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var productId = product.definition.id;
            var reason = MapFailureReason(failureDescription.reason);

            Debug.LogWarning($"{TAG} Mua thất bại '{productId}': {failureDescription.reason} — {failureDescription.message}");

            _purchaseCallback?.Invoke(IAPPurchaseResult.Failure(productId, reason, failureDescription.message));
            _purchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var productId = product.definition.id;
            var reason = MapFailureReason(failureReason);

            Debug.LogWarning($"{TAG} Mua thất bại '{productId}': {failureReason}");

            _purchaseCallback?.Invoke(IAPPurchaseResult.Failure(productId, reason, failureReason.ToString()));
            _purchaseCallback = null;
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private static ProductType ConvertProductType(IAPProductType type)
        {
            return type switch
            {
                IAPProductType.Consumable => ProductType.Consumable,
                IAPProductType.NonConsumable => ProductType.NonConsumable,
                IAPProductType.Subscription => ProductType.Subscription,
                _ => ProductType.Consumable
            };
        }

        private static IAPFailureReason MapFailureReason(PurchaseFailureReason reason)
        {
            return reason switch
            {
                PurchaseFailureReason.UserCancelled => IAPFailureReason.UserCancelled,
                PurchaseFailureReason.PaymentDeclined => IAPFailureReason.PaymentDeclined,
                PurchaseFailureReason.ProductUnavailable => IAPFailureReason.ProductUnavailable,
                PurchaseFailureReason.ExistingPurchasePending => IAPFailureReason.PurchasePending,
                PurchaseFailureReason.DuplicateTransaction => IAPFailureReason.AlreadyOwned,
                _ => IAPFailureReason.Unknown
            };
        }
    }
}
#endif
