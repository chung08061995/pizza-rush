using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DraftUtils.Ads
{
    /// <summary>
    /// Ads Manager — PersistentSingleton quản lý quảng cáo.
    /// Tự động chọn implementation dựa trên define symbol + AdConfig.SdkType.
    /// 
    /// Cách dùng:
    /// 1. Tạo AdConfig ScriptableObject (Create → DraftUtils → Ad Config)
    /// 2. Gắn AdsManager vào Bootstrap scene
    /// 3. Assign AdConfig vào Inspector
    /// 4. Gọi AdsManager.Instance.ShowRewarded(...) từ game code
    /// 
    /// Hoặc dùng static helper:
    /// <code>
    /// AdsExtensions.ShowRewarded("double_gems", rewarded =>
    /// {
    ///     if (rewarded) gems *= 2;
    /// });
    /// </code>
    /// </summary>
    public class AdsManager : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<AdsManager>
    {
        private DraftUtils.FormattedLogger _logger = new (FormattedLogger.CreateFormatForType(typeof(AdsManager)));

        [Header("Configuration")]
        [Tooltip("ScriptableObject chứa Ad IDs")]
        [SerializeField] private AdConfigSO _config;

        /// <summary>Ads service đang dùng.</summary>
        private IAdsService _service { get; set; }
        public IAdsService Service => _service;

        /// <summary>Event khi ads init xong.</summary>
        public event Action<bool> OnAdsInitialized;
        private DraftUtils.UnityMainThread _unityMainThread;

        protected override void OnAwake()
        {
            _service = CreateService();
            _service.Initialize(AdsServiceInitializeCompleted);
            _unityMainThread = UnityMainThread.Reuse(_unityMainThread, transform);
        }
        private void AdsServiceInitializeCompleted(bool success)
        {
            _logger.Log($"Init: {(success ? "OK" : "FAILED")}");
            OnAdsInitialized?.Invoke(success);
        }

        // ─── PUBLIC API ───

        /// <summary>Hiện banner.</summary>
        [Button]
        public void ShowBanner(AdBannerPosition position)
        {
            _service.ShowBanner(position);
        }

        /// <summary>Ẩn banner.</summary>
        public void HideBanner() => _service.HideBanner();

        /// <summary>
        /// Hiện interstitial.
        /// </summary>
        /// <param name="placement">Tên placement cho analytics</param>
        /// <param name="onClosed">Callback khi ad đóng</param>

        [Button]
        public void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            _service.ShowInterstitial(placement, onClosed);
        }

        /// <summary>
        /// Hiện rewarded video.
        /// </summary>
        /// <param name="placement">Tên placement cho analytics</param>
        /// <param name="onResult">true = user xem xong, được reward</param>

        [Button]
        public void ShowRewarded(string placement = "default", Action<bool> onResult = null)
        {
            _service.ShowRewarded(placement, onResult);
        }

        /// <summary>
        /// Tắt ads (vd: sau khi mua Remove Ads).
        /// Banner bị ẩn, interstitial không hiện.
        /// Rewarded vẫn hiện (user chủ động xem).
        /// </summary>

        [Button]
        public void DisableAds()
        {
            _service.AdsDisabled = true;
            _service.HideBanner();
            _service.DestroyBanner();
            _logger.Log("Ads disabled.");
        }

        /// <summary>Bật lại ads.</summary>

        [Button]
        public void EnableAds()
        {
            _service.AdsDisabled = false;
            _logger.Log("Ads enabled.");
        }

        // ─── FACTORY ───

        private IAdsService CreateService()
        {
            if (_config == null)
            {
                _logger.Log("Warning: AdConfig chưa được gán! Dùng StubAdsService.");
                return new StubAdsService();
            }
            if(_config.SdkType == AdSDKType.AdMob)
            {
                return new AdMobService(_config);
            }

            return new StubAdsService();

            switch (_config.SdkType)
            {
                case AdSDKType.UnityAds:
#if UNITY_ADS_INSTALLED
                    _logger.Log("Sử dụng Unity Ads.");
                    return new UnityAdsService(_config);
#else
                    _logger.Log("Warning: Unity Ads package chưa cài. Dùng Stub.");
                    return new StubAdsService();
#endif

                case AdSDKType.AdMob:
#if GOOGLE_ADMOB
                    _logger.Log("Sử dụng Google AdMob.");
                    return new AdMobService(_config);
#else
                    _logger.Log("Warning: Google Mobile Ads package chưa cài. Dùng Stub.");
                    return new StubAdsService();
#endif

                case AdSDKType.AppLovinMAX:
#if APPLOVIN_MAX
                    _logger.Log("Sử dụng AppLovin MAX.");
                    return new AppLovinMAXService(_config);
#else
                    _logger.Log("Warning: AppLovin MAX SDK chưa cài. Dùng Stub.");
                    return new StubAdsService();
#endif

                default:
                    _logger.Log("Warning: SDK type = None. Dùng StubAdsService.");
                    return new StubAdsService();
            }
        }
    }
}
