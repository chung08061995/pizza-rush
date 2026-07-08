using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DraftUtils.IAP
{
    /// <summary>
    /// IAP Manager — MonoBehaviour tiện lợi để quản lý IAP trong scene.
    /// Tự động chọn implementation dựa trên có cài Unity IAP hay không.
    /// 
    /// Cách dùng:
    /// 1. Gắn IAPManager vào GameObject trong Bootstrap scene
    /// 2. Cấu hình products trong Inspector
    /// 3. Gọi IAPManager.Instance.Purchase(...) từ UI
    /// 
    /// Hoặc dùng qua ServiceLocator:
    /// <code>
    /// var iap = ServiceLocator.Get&lt;IIAPService&gt;();
    /// iap.PurchaseProduct("com.game.remove_ads");
    /// </code>
    /// 
    /// Khi CHƯA cài Unity IAP:
    /// - Tự dùng StubIAPService (không crash, chỉ log warning)
    /// - Có thể build và test game bình thường
    /// 
    /// Khi ĐÃ cài Unity IAP:
    /// - Tự dùng UnityIAPService (kết nối store thật)
    /// - Symbol UNITY_IAP được tự thêm bởi package
    /// </summary>
    public class IAPManager : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<IAPManager>
    {
        [Header("IAP Products")]
        [Tooltip("Danh sách products — cấu hình trong Inspector")]
        [ShowInInspector] [SerializeField] private IAPProductInfo[] _products = Array.Empty<IAPProductInfo>();

        [Tooltip("Optional provider for projects that build products from their own data layer.")]
        [SerializeField] private MonoBehaviour _productProviderBehaviour;

        [Header("Settings")]
        [Tooltip("Tự động khởi tạo IAP khi Awake")]
        [SerializeField] private bool _autoInitialize = true;

        /// <summary>IAP service đang được sử dụng.</summary>
        public IIAPService Service { get; private set; }
        public IIAPProductProvider ProductProvider => _productProviderBehaviour as IIAPProductProvider;

        /// <summary>Event khi IAP init xong.</summary>
        public event Action<bool> OnInitialized;

        /// <summary>Event khi purchase hoàn tất.</summary>
        public event Action<IAPPurchaseResult> OnPurchaseCompleted;

        protected override void OnAwake()
        {
            Service = CreateService();
        }

        private void Start()
        {
            if (_autoInitialize)
            {
                if (_products == null || _products.Length == 0)
                {
                    LoadProductsFromProvider();
                }

                if (_products != null && _products.Length > 0)
                {
                    Initialize();
                }
            }
        }

        public void SetProductProvider(IIAPProductProvider productProvider)
        {
            if (productProvider != null && productProvider is MonoBehaviour providerBehaviour)
            {
                _productProviderBehaviour = providerBehaviour;
            }
        }

        private void LoadProductsFromProvider()
        {
            var provider = ProductProvider ?? GetComponent<IIAPProductProvider>();
            if (provider == null)
            {
                Debug.LogWarning("[IAPManager] No products configured and no IIAPProductProvider found.");
                return;
            }

            _products = provider.GetProducts() ?? Array.Empty<IAPProductInfo>();
            Debug.Log($"[IAPManager] Loaded {_products.Length} products from provider.");
        }

        /// <summary>
        /// Khởi tạo IAP với products đã cấu hình.
        /// </summary>
        public void Initialize()
        {
            Service.Initialize(_products, success =>
            {
                OnInitialized?.Invoke(success);
            });
        }

        /// <summary>
        /// Khởi tạo IAP với danh sách products tùy chỉnh.
        /// </summary>
        public void Initialize(IAPProductInfo[] products)
        {
            _products = products;
            Service.Initialize(products, success =>
            {
                OnInitialized?.Invoke(success);
            });
        }

        /// <summary>
        /// Mua product.
        /// </summary>
        /// <param name="productId">ID product trên store</param>
        /// <param name="onResult">Callback kết quả (optional, cũng fire event OnPurchaseCompleted)</param>
        public void Purchase(string productId, Action<IAPPurchaseResult> onResult = null)
        {
            Service.PurchaseProduct(productId, result =>
            {
                onResult?.Invoke(result);
                OnPurchaseCompleted?.Invoke(result);
            });
        }

        /// <summary>
        /// Restore purchases (iOS cần nút Restore trong UI).
        /// </summary>
        public void Restore(Action<bool> onComplete = null)
        {
            Service.RestorePurchases(onComplete);
        }

        /// <summary>
        /// Kiểm tra đã sở hữu product chưa (non-consumable).
        /// </summary>
        public bool IsOwned(string productId) => Service.IsProductOwned(productId);

        /// <summary>
        /// Lấy giá đã localize.
        /// </summary>
        public string GetPrice(string productId) => Service.GetLocalizedPrice(productId);

        /// <summary>
        /// Tạo service phù hợp dựa trên define symbol.
        /// </summary>
        private static IIAPService CreateService()
        {
#if UNITY_IAP
            Debug.Log("[IAPManager] Sử dụng UnityIAPService (Unity IAP đã cài).");
            return new UnityIAPService();
#else
            Debug.LogWarning("[IAPManager] Sử dụng StubIAPService — Unity IAP chưa được cài đặt.");
            return new StubIAPService();
#endif
        }
    }
}
