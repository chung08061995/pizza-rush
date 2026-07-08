using System;
using UnityEngine;

namespace DraftUtils.Ads
{
    /// <summary>
    /// Stub Ads service — dùng khi chưa cài bất kỳ Ads SDK nào.
    /// Mọi hàm đều log và trả kết quả mặc định an toàn.
    /// Rewarded luôn trả true (simulate user xem xong) để test flow.
    /// </summary>
    public class StubAdsService : IAdsService
    {
        private const string TAG = "[Ads-Stub]";

        public bool IsInitialized => false;
        public bool AdsDisabled { get; set; }
        public bool IsInterstitialReady => false;
        public bool IsRewardedReady => true; // Simulate always ready for testing

        public event Action<AdEventInfo> OnAdShown;
        public event Action<AdEventInfo> OnAdClosed;
        public event Action<AdEventInfo> OnAdFailed;
        public event Action<AdEventInfo> OnRewardEarned;

        public void Initialize(Action<bool> onComplete = null)
        {
            Debug.LogWarning($"{TAG} Ads SDK chưa cài. Cài Unity Ads / AdMob / AppLovin MAX " +
                             $"và thêm define tương ứng để kích hoạt.");
            onComplete?.Invoke(false);
        }

        // ─── BANNER ───

        public void ShowBanner(AdBannerPosition position = AdBannerPosition.Bottom)
        {
            Debug.Log($"{TAG} ShowBanner at {position} (stub — no-op)");
        }

        public void HideBanner()
        {
            Debug.Log($"{TAG} HideBanner (stub — no-op)");
        }

        public void DestroyBanner()
        {
            Debug.Log($"{TAG} DestroyBanner (stub — no-op)");
        }

        // ─── INTERSTITIAL ───

        public void LoadInterstitial()
        {
            Debug.Log($"{TAG} LoadInterstitial (stub — no-op)");
        }

        public void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            Debug.Log($"{TAG} ShowInterstitial '{placement}' (stub — simulating close)");
            onClosed?.Invoke();
            OnAdShown?.Invoke(new AdEventInfo(AdType.Interstitial, placement, "Stub"));
            OnAdClosed?.Invoke(new AdEventInfo(AdType.Interstitial, placement, "Stub"));
        }

        // ─── REWARDED ───

        public void LoadRewarded()
        {
            Debug.Log($"{TAG} LoadRewarded (stub — no-op)");
        }

        public void ShowRewarded(string placement = "default", Action<bool> onResult = null)
        {
            Debug.Log($"{TAG} ShowRewarded '{placement}' (stub — simulating reward granted)");
            onResult?.Invoke(true); // Simulate user watched full ad
            var info = new AdEventInfo(AdType.Rewarded, placement, "Stub");
            OnAdShown?.Invoke(info);
            OnRewardEarned?.Invoke(info);
            OnAdClosed?.Invoke(info);
        }
    }
}
