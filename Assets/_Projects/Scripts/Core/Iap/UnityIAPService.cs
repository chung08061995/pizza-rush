#if UNITY_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Purchasing.Security;
#endif

namespace DraftUtils.IAP
{
    /// <summary>
    /// Unity IAP v5 implementation.
    /// </summary>
    public class UnityIAPService : IIAPService
    {
        private const string TAG = "[IAP]";

        private StoreController _storeController;
        private readonly Dictionary<string, IAPProductInfo> _productInfoMap = new();
        private readonly HashSet<string> _processedOrders = new();

        private Action<bool> _initCallback;
        private Action<IAPPurchaseResult> _purchaseCallback;
        private Action<bool> _restoreCallback;
        private bool _isConnecting;
        private bool _productsFetched;
        private bool _purchasesFetched;

        public bool IsInitialized =>
            _storeController != null &&
            _productsFetched &&
            _purchasesFetched;

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
            _productsFetched = false;
            _purchasesFetched = false;

            if (products == null || products.Length == 0)
            {
                Debug.LogWarning($"{TAG} Không có product nào để khởi tạo.");
                CompleteInitialization(false);
                return;
            }

            foreach (var info in products)
            {
                if (info == null || string.IsNullOrEmpty(info.ProductId))
                {
                    continue;
                }

                _productInfoMap[info.ProductId] = info;
            }

            if (_productInfoMap.Count == 0)
            {
                Debug.LogWarning($"{TAG} Không có product hợp lệ để khởi tạo.");
                CompleteInitialization(false);
                return;
            }

            if (_storeController == null)
            {
                _storeController = UnityIAPServices.StoreController();
                RegisterCallbacks();
            }

            ConnectStore();
        }

        public void PurchaseProduct(string productId, Action<IAPPurchaseResult> onResult = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"{TAG} Chưa khởi tạo, không thể mua '{productId}'.");
                onResult?.Invoke(IAPPurchaseResult.Failure(productId, IAPFailureReason.NotInitialized));
                return;
            }

            var product = _storeController.GetProductById(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"{TAG} Product '{productId}' không khả dụng.");
                onResult?.Invoke(IAPPurchaseResult.Failure(productId, IAPFailureReason.ProductUnavailable));
                return;
            }

            _purchaseCallback = onResult;
            Debug.Log($"{TAG} Bắt đầu mua '{productId}'...");
            _storeController.PurchaseProduct(product);
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
            Debug.Log($"{TAG} Đang restore purchases...");
            _storeController.RestoreTransactions((success, error) =>
            {
                if (!success)
                {
                    Debug.LogWarning($"{TAG} Restore thất bại: {error}");
                }

                _restoreCallback?.Invoke(success);
                _restoreCallback = null;
            });
        }

        public bool IsProductOwned(string productId)
        {
            if (_storeController == null || string.IsNullOrEmpty(productId))
            {
                return false;
            }

            return _storeController.GetPurchases().Any(order =>
                order is ConfirmedOrder &&
                string.Equals(GetProductId(order), productId, StringComparison.Ordinal));
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_storeController == null)
            {
                return "N/A";
            }

            var product = _storeController.GetProductById(productId);
            return product?.metadata?.localizedPriceString ?? "N/A";
        }

        public IAPProductInfo GetProductInfo(string productId)
        {
            if (!_productInfoMap.TryGetValue(productId, out var info))
            {
                return null;
            }

            info.LocalizedPrice = GetLocalizedPrice(productId);
            return info;
        }

        private async void ConnectStore()
        {
            if (_isConnecting)
            {
                return;
            }

            _isConnecting = true;
            try
            {
                Debug.Log($"{TAG} Đang kết nối store...");
                await _storeController.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{TAG} Kết nối store thất bại: {ex.Message}");
                CompleteInitialization(false);
            }
            finally
            {
                _isConnecting = false;
            }
        }

        private void RegisterCallbacks()
        {
            _storeController.OnStoreConnected += HandleStoreConnected;
            _storeController.OnStoreDisconnected += HandleStoreDisconnected;

            _storeController.OnProductsFetched += HandleProductsFetched;
            _storeController.OnProductsFetchFailed += HandleProductsFetchFailed;

            _storeController.OnPurchasePending += HandlePurchasePending;
            _storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
            _storeController.OnPurchaseFailed += HandlePurchaseFailed;
            _storeController.OnPurchaseDeferred += HandlePurchaseDeferred;

            _storeController.OnPurchasesFetched += HandlePurchasesFetched;
            _storeController.OnPurchasesFetchFailed += HandlePurchasesFetchFailed;
        }

        private void HandleStoreConnected()
        {
            Debug.Log($"{TAG} Kết nối store thành công. Fetch {_productInfoMap.Count} products...");
            _storeController.FetchProducts(BuildProductDefinitions());
        }

        private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            Debug.LogWarning($"{TAG} Store disconnected: {failure.Message}");
            CompleteInitialization(false);
        }

        private void HandleProductsFetched(List<Product> products)
        {
            foreach (var product in products)
            {
                if (_productInfoMap.TryGetValue(product.definition.id, out var info))
                {
                    info.LocalizedPrice = product.metadata.localizedPriceString;
                }
            }

            _productsFetched = true;
            Debug.Log($"{TAG} Fetch products thành công: {products.Count}.");
            _storeController.FetchPurchases();
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogError($"{TAG} Fetch products thất bại: {failure.FailureReason}");
            CompleteInitialization(false);
        }

        private void HandlePurchasePending(PendingOrder order)
        {
            var productId = GetProductId(order);
            var orderKey = GetOrderKey(order);

            if (!string.IsNullOrEmpty(orderKey) && _processedOrders.Contains(orderKey))
            {
                Debug.Log($"{TAG} Bỏ qua pending order đã xử lý: {productId}");
                _storeController.ConfirmPurchase(order);
                return;
            }

            Debug.Log($"{TAG} Purchase pending: {productId}");

            if (!ValidateReceipt(order.Info.Receipt, productId))
            {
                _purchaseCallback?.Invoke(IAPPurchaseResult.Failure(
                    productId,
                    IAPFailureReason.Unknown,
                    "Google Play receipt validation failed."));
                _purchaseCallback = null;
                return;
            }

            if (!string.IsNullOrEmpty(orderKey))
            {
                _processedOrders.Add(orderKey);
            }

            _purchaseCallback?.Invoke(IAPPurchaseResult.Success(
                productId,
                order.Info.Receipt,
                order.Info.TransactionID));
            _purchaseCallback = null;

            _storeController.ConfirmPurchase(order);
        }

        private static bool ValidateReceipt(string receipt, string productId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!GooglePlayTangle.IsPopulated || string.IsNullOrEmpty(receipt))
            {
                Debug.LogError($"{TAG} GooglePlayTangle/receipt chưa hợp lệ cho '{productId}'.");
                return false;
            }

            try
            {
                var validator = new CrossPlatformValidator(
                    GooglePlayTangle.Data(),
                    Application.identifier);
                validator.Validate(receipt);
                return true;
            }
            catch (IAPSecurityException ex)
            {
                Debug.LogError($"{TAG} Receipt validation thất bại cho '{productId}': {ex.Message}");
                return false;
            }
#else
            return true;
#endif
        }

        private void HandlePurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case ConfirmedOrder confirmedOrder:
                    Debug.Log($"{TAG} Purchase confirmed: {GetProductId(confirmedOrder)}");
                    break;
                case FailedOrder failedOrder:
                    HandlePurchaseFailure(failedOrder);
                    break;
            }
        }

        private void HandlePurchaseFailed(FailedOrder failedOrder)
        {
            HandlePurchaseFailure(failedOrder);
        }

        private void HandlePurchaseDeferred(DeferredOrder order)
        {
            Debug.Log($"{TAG} Purchase deferred: {GetProductId(order)}");
        }

        private void HandlePurchasesFetched(Orders orders)
        {
            Debug.Log($"{TAG} Existing purchases fetched. Confirmed: {orders.ConfirmedOrders.Count}, Pending: {orders.PendingOrders.Count}, Deferred: {orders.DeferredOrders.Count}");
            _purchasesFetched = true;
            CompleteInitialization(true);
        }

        private void HandlePurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"{TAG} Fetch purchases thất bại: {failure.FailureReason} - {failure.Message}");
            // Product metadata is already usable. Do not block new purchases just
            // because Play could not restore ownership during this session.
            _purchasesFetched = true;
            CompleteInitialization(true);
        }

        private void HandlePurchaseFailure(FailedOrder failedOrder)
        {
            var productId = GetProductId(failedOrder);
            var reason = MapFailureReason(failedOrder.FailureReason);

            Debug.LogWarning($"{TAG} Purchase failed '{productId}': {failedOrder.FailureReason} - {failedOrder.Details}");

            _purchaseCallback?.Invoke(IAPPurchaseResult.Failure(productId, reason, failedOrder.Details));
            _purchaseCallback = null;
        }

        private List<ProductDefinition> BuildProductDefinitions()
        {
            return _productInfoMap.Values
                .Select(info => new ProductDefinition(info.ProductId, ConvertProductType(info.ProductType)))
                .ToList();
        }

        private void CompleteInitialization(bool success)
        {
            _initCallback?.Invoke(success);
            _initCallback = null;
        }

        private static string GetProductId(Order order)
        {
            return order?.CartOrdered?.Items()?.FirstOrDefault()?.Product?.definition?.id ?? string.Empty;
        }

        private static string GetOrderKey(Order order)
        {
            if (order == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(order.Info.TransactionID))
            {
                return order.Info.TransactionID;
            }

            return $"{GetProductId(order)}:{order.Info.Receipt}";
        }

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
