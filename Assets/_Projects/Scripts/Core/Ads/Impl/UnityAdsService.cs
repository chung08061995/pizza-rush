#if UNITY_ADS_INSTALLED
using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace DraftUtils.Ads
{
    /// <summary>
    /// Unity Ads implementation.
    /// Chỉ compile khi có define UNITY_ADS_INSTALLED (từ com.unity.ads).
    /// 
    /// Unity Ads dùng Game ID thay cho App ID.
    /// Ad Unit IDs: Banner_Android, Interstitial_Android, Rewarded_Android (default).
    /// </summary>
    public class UnityAdsService : IAdsService, IUnityAdsInitializationListener,
        IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private const string TAG = "[Ads-UnityAds]";
        private readonly AdConfig _config;

        private Action<bool> _initCallback;
        private Action _interstitialClosedCallback;
        private Action<bool> _rewardedCallback;
        private bool _rewardGranted;

        public bool IsInitialized => Advertisement.isInitialized;
        public bool AdsDisabled { get; set; }
        public bool IsInterstitialReady { get; private set; }
        public bool IsRewardedReady { get; private set; }

        public event Action<AdEventInfo> OnAdShown;
        public event Action<AdEventInfo> OnAdClosed;
        public event Action<AdEventInfo> OnAdFailed;
        public event Action<AdEventInfo> OnRewardEarned;

        public UnityAdsService(AdConfig config)
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
            var gameId = _config.AppId;
            Debug.Log($"{TAG} Initializing with Game ID: {gameId}, TestMode: {_config.TestMode}");
            Advertisement.Initialize(gameId, _config.TestMode, this);
        }

        // ─── IUnityAdsInitializationListener ───

        public void OnInitializationComplete()
        {
            Debug.Log($"{TAG} Initialized successfully!");
            _initCallback?.Invoke(true);
            _initCallback = null;

            // Auto-load ads
            LoadInterstitial();
            LoadRewarded();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"{TAG} Init failed: {error} — {message}");
            _initCallback?.Invoke(false);
            _initCallback = null;
        }

        // ─── BANNER ───

        public void ShowBanner(AdBannerPosition position = AdBannerPosition.Bottom)
        {
            if (AdsDisabled) return;

            var bannerPos = position == AdBannerPosition.Top
                ? BannerPosition.TOP_CENTER
                : BannerPosition.BOTTOM_CENTER;

            Advertisement.Banner.SetPosition(bannerPos);
            Advertisement.Banner.Show(_config.BannerId);
            Debug.Log($"{TAG} Banner shown at {position}");
        }

        public void HideBanner()
        {
            Advertisement.Banner.Hide();
        }

        public void DestroyBanner()
        {
            Advertisement.Banner.Hide(true);
        }

        // ─── INTERSTITIAL ───

        public void LoadInterstitial()
        {
            Advertisement.Load(_config.InterstitialId, this);
        }

        public void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            if (AdsDisabled) { onClosed?.Invoke(); return; }

            if (!IsInterstitialReady)
            {
                Debug.LogWarning($"{TAG} Interstitial not ready.");
                onClosed?.Invoke();
                return;
            }

            _interstitialClosedCallback = onClosed;
            IsInterstitialReady = false;
            Advertisement.Show(_config.InterstitialId, this);
        }

        // ─── REWARDED ───

        public void LoadRewarded()
        {
            Advertisement.Load(_config.RewardedId, this);
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
                return;
            }

            _rewardedCallback = onResult;
            _rewardGranted = false;
            IsRewardedReady = false;
            Advertisement.Show(_config.RewardedId, this);
        }

        // ─── IUnityAdsLoadListener ───

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            if (adUnitId == _config.InterstitialId)
                IsInterstitialReady = true;
            else if (adUnitId == _config.RewardedId)
                IsRewardedReady = true;

            Debug.Log($"{TAG} Ad loaded: {adUnitId}");
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"{TAG} Load failed '{adUnitId}': {error} — {message}");
            OnAdFailed?.Invoke(new AdEventInfo
            {
                AdType = adUnitId == _config.RewardedId ? AdType.Rewarded : AdType.Interstitial,
                AdNetwork = "UnityAds",
                ErrorMessage = message
            });
        }

        // ─── IUnityAdsShowListener ───

        public void OnUnityAdsShowStart(string adUnitId)
        {
            var type = adUnitId == _config.RewardedId ? AdType.Rewarded : AdType.Interstitial;
            OnAdShown?.Invoke(new AdEventInfo(type, "", "UnityAds"));
        }

        public void OnUnityAdsShowClick(string adUnitId) { }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            if (adUnitId == _config.RewardedId)
            {
                _rewardGranted = showCompletionState == UnityAdsShowCompletionState.COMPLETED;
                _rewardedCallback?.Invoke(_rewardGranted);
                _rewardedCallback = null;

                if (_rewardGranted)
                    OnRewardEarned?.Invoke(new AdEventInfo(AdType.Rewarded, "", "UnityAds"));

                OnAdClosed?.Invoke(new AdEventInfo(AdType.Rewarded, "", "UnityAds"));
                LoadRewarded(); // Auto-reload
            }
            else if (adUnitId == _config.InterstitialId)
            {
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Interstitial, "", "UnityAds"));
                LoadInterstitial(); // Auto-reload
            }
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            Debug.LogWarning($"{TAG} Show failed '{adUnitId}': {error} — {message}");

            if (adUnitId == _config.RewardedId)
            {
                _rewardedCallback?.Invoke(false);
                _rewardedCallback = null;
            }
            else
            {
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
            }

            OnAdFailed?.Invoke(new AdEventInfo
            {
                AdType = adUnitId == _config.RewardedId ? AdType.Rewarded : AdType.Interstitial,
                AdNetwork = "UnityAds",
                ErrorMessage = message
            });
        }
    }
}
#endif
