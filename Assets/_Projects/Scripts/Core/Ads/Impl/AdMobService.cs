
using System;
using System.Collections.Generic;
using UnityEngine;
#if GOOGLE_ADMOB
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
#if GOOGLE_ADMOB
    public class AdMobService : IAdsService
    {
        private const string TestBannerId = "ca-app-pub-3940256099942544/6300978111";
        private const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        private const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";
        private readonly DraftUtils.FormattedLogger _logger = new(DraftUtils.FormattedLogger.CreateFormatForType(typeof(AdMobService)));
        private readonly AdConfigSO _config;

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;

        private Action _interstitialClosedCallback;
        private Action<bool> _rewardedCallback;
        private bool _rewardGranted;
        private string _interstitialPlacement = "default";
        private string _rewardedPlacement = "default";

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

            _logger.Log("Initializing AdMob; requesting UMP consent...");
            AdPrivacyController.RequestConsent(canRequest =>
            {
                if (!canRequest) { _logger.Log("Ads unavailable until privacy consent is granted."); onComplete?.Invoke(false); return; }
                MobileAds.RaiseAdEventsOnUnityMainThread = true;
                MobileAds.Initialize(status =>
                {
                    IsInitialized = status != null;
                    _logger.Log(IsInitialized ? "AdMob initialized!" : "AdMob initialization failed.");
                    onComplete?.Invoke(IsInitialized);
                    if (IsInitialized) { LoadInterstitial(); LoadRewarded(); }
                });
            });
        }

        // ─── BANNER ───

        public void ShowBanner(AdBannerPosition position = AdBannerPosition.Bottom)
        {
            if (AdsDisabled || !AdPrivacyController.CanRequestAds) return;

            DestroyBanner();

            var adMobPos = position == AdBannerPosition.Top
                ? AdPosition.Top
                : AdPosition.Bottom;

            var id = _config.TestMode ? TestBannerId : _config.platformConfig.bannerId;
            _bannerView = new BannerView(id, AdSize.Banner, adMobPos);
            _bannerView.OnAdPaid += value => LogPaid("banner", "gameplay", value);

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
            var id = _config.TestMode ? TestInterstitialId : _config.platformConfig.interstitialId;
            InterstitialAd.Load(id, request, (ad, error) =>
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
            if (AdsDisabled || !AdPrivacyController.CanRequestAds) { onClosed?.Invoke(); return; }

            if (!IsInterstitialReady)
            {
                _logger.Log("Interstitial not ready.");
                onClosed?.Invoke();
                LoadInterstitial();
                return;
            }

            _interstitialClosedCallback = onClosed;
            _interstitialPlacement = placement;
            _interstitialAd.Show();
            OnAdShown?.Invoke(new AdEventInfo(AdType.Interstitial, placement, "AdMob"));
        }

        private void RegisterInterstitialEvents(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _interstitialClosedCallback?.Invoke();
                _interstitialClosedCallback = null;
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Interstitial, _interstitialPlacement, "AdMob"));
                LoadInterstitial(); // Auto-reload
            };
            ad.OnAdPaid += value => LogPaid("interstitial", _interstitialPlacement, value);

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
            var id = _config.TestMode ? TestRewardedId : _config.platformConfig.rewardedId;
            RewardedAd.Load(id, request, (ad, error) =>
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
            _rewardedPlacement = placement;
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
                OnAdClosed?.Invoke(new AdEventInfo(AdType.Rewarded, _rewardedPlacement, "AdMob"));
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
            ad.OnAdPaid += value => LogPaid("rewarded", _rewardedPlacement, value);
        }

        private static void LogPaid(string adType, string placement, AdValue value)
        {
            GameAnalytics.Log("ad_paid", new Dictionary<string, object>
            {
                { "ad_type", adType }, { "placement", placement ?? string.Empty },
                { "currency", value.CurrencyCode ?? string.Empty }, { "value", value.Value / 1000000d },
            });
        }
    }
#else
    public class AdMobService : StubAdsService
    {
        public AdMobService(AdConfigSO config) { }
    }
#endif
}
