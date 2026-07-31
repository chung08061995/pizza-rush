using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using DraftUtils.IAP;

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
        /// <summary>Event khi quyền No Ads được cấp hoặc khôi phục.</summary>
        public event Action OnNoAdsEntitlementChanged;
        public bool HasNoAds => PlayerPrefs.GetInt(GameConstain.PlayerPrefsKey.NoAdsOwned, 0) == 1;
        public bool AdsDisabled => _service != null && _service.AdsDisabled;
        private DraftUtils.UnityMainThread _unityMainThread;
        private int _winInterstitialCounter;
        private int _loseInterstitialCounter;
        private float _lastInterstitialTime;

        protected override void OnAwake()
        {
            _service = CreateService();
            _service.OnAdShown += HandleAdShown;
            _service.OnAdClosed += info => LogAdEvent("ad_close", info);
            _service.OnAdFailed += info => LogAdEvent("ad_fail", info);
            _service.OnRewardEarned += info => LogAdEvent("ad_reward", info);

            if (HasNoAds)
            {
                _service.AdsDisabled = true;
            }

            _service.Initialize(AdsServiceInitializeCompleted);
            _unityMainThread = UnityMainThread.Reuse(_unityMainThread, transform);
        }

        private void Start()
        {
            var iapManager = IAPManager.Instance;
            if (iapManager == null)
            {
                _logger.Log("Warning: IAPManager not found; No Ads ownership cannot be restored.");
                return;
            }

            iapManager.OnInitialized += HandleIAPInitialized;
            iapManager.OnPurchaseCompleted += HandlePurchaseCompleted;

            if (iapManager.Service.IsInitialized)
            {
                RestoreNoAdsOwnership(iapManager);
            }
        }

        private void HandleIAPInitialized(bool success)
        {
            if (success)
            {
                RestoreNoAdsOwnership(IAPManager.Instance);
            }
        }

        private void HandlePurchaseCompleted(IAPPurchaseResult result)
        {
            if (result.IsSuccess &&
                string.Equals(result.ProductId, GameConstain.IAPProductId.NoAds, StringComparison.Ordinal))
            {
                GrantNoAdsEntitlement();
            }
        }

        private void RestoreNoAdsOwnership(IAPManager iapManager)
        {
            if (iapManager != null && iapManager.IsOwned(GameConstain.IAPProductId.NoAds))
            {
                GrantNoAdsEntitlement();
            }
        }

        private void GrantNoAdsEntitlement()
        {
            PlayerPrefs.SetInt(GameConstain.PlayerPrefsKey.NoAdsOwned, 1);
            PlayerPrefs.Save();
            DisableAds();
            OnNoAdsEntitlementChanged?.Invoke();
        }
        private void AdsServiceInitializeCompleted(bool success)
        {
            _logger.Log($"Init: {(success ? "OK" : "FAILED")}");
            OnAdsInitialized?.Invoke(success);
        }

        private void HandleAdShown(AdEventInfo info)
        {
            if (info != null && info.AdType == AdType.Interstitial)
            {
                _lastInterstitialTime = Time.realtimeSinceStartup;
            }

            LogAdEvent("ad_show", info);
        }

        // ─── PUBLIC API ───

        /// <summary>Hiện banner.</summary>
        [Button]
        public void ShowBanner(AdBannerPosition position)
        {
            if (_service == null || _service.AdsDisabled || !MonetizationConfig.CanShowBanner(DataManager.Instance != null ? DataManager.Instance.Level.Value : 1)) return;
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
            if (_service == null || _service.AdsDisabled)
            {
                onClosed?.Invoke();
                return;
            }

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
            if (_service == null || !MonetizationConfig.RewardedEnabled)
            {
                onResult?.Invoke(false);
                return;
            }
            _service.ShowRewarded(placement, onResult);
        }

        public void ShowLevelEndInterstitial(bool won, Action onComplete)
        {
            int level = DataManager.Instance != null ? DataManager.Instance.Level.Value - (won ? 1 : 0) : 1;
            int counter = won ? ++_winInterstitialCounter : ++_loseInterstitialCounter;
            int interval = won ? MonetizationConfig.InterstitialWinInterval : MonetizationConfig.InterstitialLoseInterval;
            bool cooldownPassed = Time.realtimeSinceStartup - _lastInterstitialTime >= MonetizationConfig.InterstitialCooldownSeconds;
            if (!MonetizationConfig.CanShowInterstitial(level) || counter % interval != 0 || !cooldownPassed)
            {
                onComplete?.Invoke();
                return;
            }

            ShowInterstitial(won ? "level_win" : "level_lose", onComplete);
        }

        public void ShowPrivacyOptions() => AdPrivacyController.ShowPrivacyOptions();

        private static void LogAdEvent(string eventName, AdEventInfo info)
        {
            if (info == null) return;
            GameAnalytics.Log(eventName, new Dictionary<string, object>
            {
                { "ad_type", info.AdType.ToString() }, { "placement", info.Placement ?? string.Empty },
                { "network", info.AdNetwork ?? string.Empty }, { "error", info.ErrorMessage ?? string.Empty },
                { "value", info.Revenue }, { "currency", info.RevenueCurrency ?? string.Empty },
            });
        }

        /// <summary>
        /// Tắt ads (vd: sau khi mua Remove Ads).
        /// Banner/interstitial bị tắt; rewarded vẫn hoạt động khi người chơi chủ động chọn.
        /// </summary>

        [Button]
        public void DisableAds()
        {
            if (_service == null) return;
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
