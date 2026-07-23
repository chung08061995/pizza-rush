#if APPLOVIN_MAX
using System;
using UnityEngine;

namespace DraftUtils.Ads
{
    /// <summary>
    /// AppLovin MAX implementation.
    /// Chỉ compile khi có define APPLOVIN_MAX.
    /// 
    /// AppLovin MAX là mediation platform — tự chọn network trả giá cao nhất.
    /// Cần: MAX SDK Key + Ad Unit IDs cho mỗi ad type.
    /// </summary>
    public class AppLovinMAXService : IAdsService
    {
        private const string TAG = "[Ads-MAX]";
        private readonly AdConfig _config;

        private Action<bool> _initCallback;
        private Action _interstitialClosedCallback;
        private Action<bool> _rewardedCallback;
        private bool _rewardGranted;
        private string _currentInterstitialPlacement;
        private string _currentRewardedPlacement;

        public bool IsInitialized { get; private set; }
        public bool AdsDisabled { get; set; }
        public bool IsInterstitialReady => MaxSdk.IsInterstitialReady(_config.InterstitialId);
        public bool IsRewardedReady => MaxSdk.IsRewardedAdReady(_config.RewardedId);

        public event Action<AdEventInfo> OnAdShown;
        public event Action<AdEventInfo> OnAdClosed;
        public event Action<AdEventInfo> OnAdFailed;
        public event Action<AdEventInfo> OnRewardEarned;

        public AppLovinMAXService(AdConfig config)
        {
            _config = config;
        }

        public void Initialize(Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                onComplete?.Invoke(true);
                return;
            }

            _initCallback = onComplete;

            Debug.Log($"{TAG} Initializing MAX SDK...");
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;
            MaxSdk.SetSdkKey(_config.MaxSdkKey);

            if (_config.TestMode)
            {
                MaxSdk.ShowMediationDebugger();
            }

            MaxSdk.InitializeSdk();
        }

        private void OnSdkInitialized(MaxSdkBase.SdkConfiguration sdkConfig)
        {
            IsInitialized = true;
            Debug.Log($"{TAG} MAX SDK initialized! Mediation: {sdkConfig.AppTrackingStatus}");

            // Register callbacks
            RegisterInterstitialCallbacks();
            RegisterRewardedCallbacks();
            RegisterBannerCallbacks();

            // Auto-load
            LoadInterstitial();
            LoadRewarded();

            _initCallback?.Invoke(true);
            _initCallback = null;
        }

        // ─── BANNER ───

        public void ShowBanner(AdBannerPosition position = AdBannerPosition.Bottom)
        {
            if (AdsDisabled) return;

            var maxPos = position == AdBannerPosition.Top
                ? MaxSdkBase.BannerPosition.TopCenter
                : MaxSdkBase.BannerPosition.BottomCenter;

            MaxSdk.CreateBanner(_config.BannerId, maxPos);
            MaxSdk.SetBannerBackgroundColor(_config.BannerId, Color.clear);
            MaxSdk.ShowBanner(_config.BannerId);
            Debug.Log($"{TAG} Banner shown at {position}");
        }

        public void HideBanner()
        {
            MaxSdk.HideBanner(_config.BannerId);
        }

        public void DestroyBanner()
        {
            MaxSdk.DestroyBanner(_config.BannerId);
        }

        // ─── INTERSTITIAL ───

        public void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(_config.InterstitialId);
        }

        public void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            if (AdsDisabled) { onClosed?.Invoke(); return; }

            if (!IsInterstitialReady)
            {
                Debug.LogWarning($"{TAG} Interstitial not ready.");
                onClosed?.Invoke();
                LoadInterstitial();
                return;
            }

            _interstitialClosedCallback = onClosed;
            _currentInterstitialPlacement = placement;
            MaxSdk.ShowInterstitial(_config.InterstitialId, placement);
        }

        private void RegisterInterstitialCallbacks()
        {
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (adUnitId, adInfo) =>
            {
                OnAdShown?.Invoke(new AdEventInfo(AdType.Interstitial, _currentInterstitialPlacement, "MAX")
                {
                    Revenue = adInfo.Revenue,
                    RevenueCurrency = "USD"
                });
            };

            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (adUnitId, adInfo) =>
            {
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Interstitial, _currentInterstitialPlacement, "MAX"));
                LoadInterstitial(); // Auto-reload
            };

            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (adUnitId, errorInfo, adInfo) =>
            {
                Debug.LogWarning($"{TAG} Interstitial display failed: {errorInfo.Message}");
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
                OnAdFailed?.Invoke(new AdEventInfo
                {
                    AdType = AdType.Interstitial,
                    AdNetwork = "MAX",
                    Placement = _currentInterstitialPlacement,
                    ErrorMessage = errorInfo.Message
                });
                LoadInterstitial();
            };

            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (adUnitId, errorInfo) =>
            {
                Debug.LogWarning($"{TAG} Interstitial load failed: {errorInfo.Message}");
                // Retry after delay
            };

            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
            {
                TrackAdRevenue(AdType.Interstitial, adInfo);
            };
        }

        // ─── REWARDED ───

        public void LoadRewarded()
        {
            MaxSdk.LoadRewardedAd(_config.RewardedId);
        }

        public void ShowRewarded(string placement = "default", Action<bool> onResult = null)
        {
            if (AdsDisabled)
            {
                onResult?.Invoke(false);
                return;
            }

            if (!IsRewardedReady)
            {
                Debug.LogWarning($"{TAG} Rewarded not ready.");
                onResult?.Invoke(false);
                LoadRewarded();
                return;
            }

            _rewardedCallback = onResult;
            _rewardGranted = false;
            _currentRewardedPlacement = placement;
            MaxSdk.ShowRewardedAd(_config.RewardedId, placement);
        }

        private void RegisterRewardedCallbacks()
        {
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (adUnitId, adInfo) =>
            {
                OnAdShown?.Invoke(new AdEventInfo(AdType.Rewarded, _currentRewardedPlacement, "MAX")
                {
                    Revenue = adInfo.Revenue,
                    RevenueCurrency = "USD"
                });
            };

            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (adUnitId, reward, adInfo) =>
            {
                _rewardGranted = true;
                Debug.Log($"{TAG} Reward received: {reward.Label} x{reward.Amount}");
                OnRewardEarned?.Invoke(new AdEventInfo(AdType.Rewarded, _currentRewardedPlacement, "MAX")
                {
                    Revenue = adInfo.Revenue,
                    RevenueCurrency = "USD"
                });
            };

            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (adUnitId, adInfo) =>
            {
                _rewardedCallback?.Invoke(_rewardGranted);
                _rewardedCallback = null;
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Rewarded, _currentRewardedPlacement, "MAX"));
                LoadRewarded(); // Auto-reload
            };

            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (adUnitId, errorInfo, adInfo) =>
            {
                Debug.LogWarning($"{TAG} Rewarded display failed: {errorInfo.Message}");
                _rewardedCallback?.Invoke(false);
                _rewardedCallback = null;
                OnAdFailed?.Invoke(new AdEventInfo
                {
                    AdType = AdType.Rewarded,
                    AdNetwork = "MAX",
                    Placement = _currentRewardedPlacement,
                    ErrorMessage = errorInfo.Message
                });
                LoadRewarded();
            };

            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
            {
                TrackAdRevenue(AdType.Rewarded, adInfo);
            };
        }

        // ─── BANNER CALLBACKS ───

        private void RegisterBannerCallbacks()
        {
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
            {
                TrackAdRevenue(AdType.Banner, adInfo);
            };
        }

        // ─── AD REVENUE TRACKING ───

        private void TrackAdRevenue(AdType adType, MaxSdkBase.AdInfo adInfo)
        {
            // Impression-level ad revenue — gửi lên Firebase Analytics
            Debug.Log($"{TAG} Ad revenue: ${adInfo.Revenue:F6} from {adInfo.NetworkName}");
        }
    }
}
#endif
