
using System;
using UnityEngine;
#if GOOGLE_ADMOB && !UNITY_IOS
using GoogleMobileAds.Api;
#endif

namespace DraftUtils.Ads
{
    /// <summary>
    /// Google AdMob implementation.
    /// Chỉ compile khi có define GOOGLE_ADMOB (từ com.google.ads.mobile).
    /// 
    /// Cần setup:
    /// 1. Cài Google Mobile Ads Unity Plugin
    /// 2. Set App ID trong Assets → Google Mobile Ads → Settings
    /// 3. Ad Unit IDs trong AdConfig
    /// </summary>
#if UNITY_IOS
    // Google Mobile Ads 11.2 assemblies in this project do not expose the
    // public ad API to Unity's iOS player compilation. Keep TestFlight builds
    // functional with the safe stub until the SDK package is re-imported.
    public class AdMobService : StubAdsService
    {
        public AdMobService(AdConfigSO config)
        {
        }
    }
#else
    public class AdMobService : IAdsService
    {
        private readonly DraftUtils.FormattedLogger _logger = new(DraftUtils.FormattedLogger.CreateFormatForType(typeof(AdMobService)));
        private readonly AdConfigSO _config;

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;

        private Action _interstitialClosedCallback;
        private Action<bool> _rewardedCallback;
        private bool _rewardGranted;

        public bool IsInitialized { get; private set; }
        public bool AdsDisabled { get; set; }
        public bool IsInterstitialReady => _interstitialAd != null && _interstitialAd.CanShowAd();
        public bool IsRewardedReady => _rewardedAd != null && _rewardedAd.CanShowAd();

        public event Action<AdEventInfo> OnAdShown;
        public event Action<AdEventInfo> OnAdClosed;
        public event Action<AdEventInfo> OnAdFailed;
        public event Action<AdEventInfo> OnRewardEarned;

        public AdMobService(AdConfigSO config)
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

            _logger.Log("Initializing AdMob...");
            MobileAds.Initialize(status =>
            {
                IsInitialized = true;
                _logger.Log("AdMob initialized!");
                onComplete?.Invoke(true);

                // Auto-load
                LoadInterstitial();
                LoadRewarded();
            });
        }

        // ─── BANNER ───

        public void ShowBanner(AdBannerPosition position = AdBannerPosition.Bottom)
        {
            if (AdsDisabled) return;

            DestroyBanner();

            var adMobPos = position == AdBannerPosition.Top
                ? AdPosition.Top
                : AdPosition.Bottom;

            _bannerView = new BannerView(_config.platformConfig.bannerId, AdSize.Banner, adMobPos);

            var request = new AdRequest();
            _bannerView.LoadAd(request);
            _logger.Log($"Banner shown at {position}");
        }

        public void HideBanner()
        {
            _bannerView?.Hide();
        }

        public void DestroyBanner()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        // ─── INTERSTITIAL ───

        public void LoadInterstitial()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            var request = new AdRequest();
            InterstitialAd.Load(_config.platformConfig.interstitialId, request, (ad, error) =>
            {
                if (error != null)
                {
                    _logger.Log($"Interstitial load failed: {error.GetMessage()}");
                    OnAdFailed?.Invoke(new AdEventInfo
                    {
                        AdType = AdType.Interstitial,
                        AdNetwork = "AdMob",
                        ErrorMessage = error.GetMessage()
                    });
                    return;
                }

                _interstitialAd = ad;
                RegisterInterstitialEvents(ad);
                _logger.Log("Interstitial loaded.");
            });
        }

        public void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            if (AdsDisabled) { onClosed?.Invoke(); return; }

            if (!IsInterstitialReady)
            {
                _logger.Log("Interstitial not ready.");
                onClosed?.Invoke();
                LoadInterstitial();
                return;
            }

            _interstitialClosedCallback = onClosed;
            _interstitialAd.Show();
            OnAdShown?.Invoke(new AdEventInfo(AdType.Interstitial, placement, "AdMob"));
        }

        private void RegisterInterstitialEvents(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Interstitial, "", "AdMob"));
                LoadInterstitial(); // Auto-reload
            };

            ad.OnAdFullScreenContentFailed += (error) =>
            {
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
                OnAdFailed?.Invoke(new AdEventInfo
                {
                    AdType = AdType.Interstitial,
                    AdNetwork = "AdMob",
                    ErrorMessage = error.GetMessage()
                });
                LoadInterstitial();
            };
        }

        // ─── REWARDED ───

        public void LoadRewarded()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            var request = new AdRequest();
            RewardedAd.Load(_config.platformConfig.rewardedId, request, (ad, error) =>
            {
                if (error != null)
                {
                    _logger.Log($"Rewarded load failed: {error.GetMessage()}");
                    OnAdFailed?.Invoke(new AdEventInfo
                    {
                        AdType = AdType.Rewarded,
                        AdNetwork = "AdMob",
                        ErrorMessage = error.GetMessage()
                    });
                    return;
                }

                _rewardedAd = ad;
                RegisterRewardedEvents(ad);
                _logger.Log("Rewarded loaded.");
            });
        }

        public void ShowRewarded(string placement = "default", Action<bool> onResult = null)
        {
            if (!IsRewardedReady)
            {
                _logger.Log("Rewarded not ready.");
                onResult?.Invoke(false);
                LoadRewarded();
                return;
            }

            _rewardedCallback = onResult;
            _rewardGranted = false;

            _rewardedAd.Show(reward =>
            {
                _rewardGranted = true;
                _logger.Log($"Reward earned: {reward.Amount} {reward.Type}");
                OnRewardEarned?.Invoke(new AdEventInfo(AdType.Rewarded, placement, "AdMob")
                {
                    Revenue = reward.Amount
                });
            });

            OnAdShown?.Invoke(new AdEventInfo(AdType.Rewarded, placement, "AdMob"));
        }

        private void RegisterRewardedEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _rewardedCallback?.Invoke(_rewardGranted);
                _rewardedCallback = null;
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Rewarded, "", "AdMob"));
                LoadRewarded(); // Auto-reload
            };

            ad.OnAdFullScreenContentFailed += (error) =>
            {
                _rewardedCallback?.Invoke(false);
                _rewardedCallback = null;
                OnAdFailed?.Invoke(new AdEventInfo
                {
                    AdType = AdType.Rewarded,
                    AdNetwork = "AdMob",
                    ErrorMessage = error.GetMessage()
                });
                LoadRewarded();
            };
        }
    }
#endif
}
