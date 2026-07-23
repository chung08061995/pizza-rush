using System;
using UnityEngine;

namespace DraftUtils.Ads
{
    /// <summary>
    /// Static helper cho Ads — gọi từ bất kỳ đâu không cần reference.
    /// An toàn: trả false/no-op nếu service chưa đăng ký.
    /// 
    /// <code>
    /// // Hiện rewarded
    /// AdsExtensions.ShowRewarded("double_reward", rewarded =>
    /// {
    ///     if (rewarded) GiveDoubleReward();
    /// });
    /// 
    /// // Hiện interstitial sau khi xong level
    /// AdsExtensions.ShowInterstitial("level_complete", () =>
    /// {
    ///     LoadNextLevel();
    /// });
    /// 
    /// // Check trước khi hiện button "Watch Ad"
    /// watchAdButton.gameObject.SetActive(AdsExtensions.IsRewardedReady());
    /// </code>
    /// </summary>
    public static class AdsExtensions
    {
        // ─── BANNER ───

        /// <summary>Hiện banner ad.</summary>
        public static void ShowBanner(AdBannerPosition position = AdBannerPosition.Bottom)
        {
            var service = GetService();
            if (service == null || service.AdsDisabled) return;
            service.ShowBanner(position);
        }

        /// <summary>Ẩn banner.</summary>
        public static void HideBanner()
        {
            GetService()?.HideBanner();
        }

        /// <summary>Destroy banner.</summary>
        public static void DestroyBanner()
        {
            GetService()?.DestroyBanner();
        }

        // ─── INTERSTITIAL ───

        /// <summary>Interstitial sẵn sàng hiện chưa.</summary>
        public static bool IsInterstitialReady()
        {
            return GetService()?.IsInterstitialReady ?? false;
        }

        /// <summary>Load interstitial trước.</summary>
        public static void LoadInterstitial()
        {
            GetService()?.LoadInterstitial();
        }

        /// <summary>
        /// Hiện interstitial. Tự skip nếu ads disabled hoặc chưa load.
        /// </summary>
        /// <param name="placement">Placement name cho analytics</param>
        /// <param name="onClosed">Callback khi ad đóng (hoặc skip)</param>
        public static void ShowInterstitial(string placement = "default", Action onClosed = null)
        {
            var service = GetService();
            if (service == null || service.AdsDisabled)
            {
                onClosed?.Invoke();
                return;
            }
            service.ShowInterstitial(placement, onClosed);
        }

        // ─── REWARDED ───

        /// <summary>Rewarded video sẵn sàng hiện chưa.</summary>
        public static bool IsRewardedReady()
        {
            var service = GetService();
            return service != null && !service.AdsDisabled && service.IsRewardedReady;
        }

        /// <summary>Load rewarded trước.</summary>
        public static void LoadRewarded()
        {
            var service = GetService();
            if (service == null || service.AdsDisabled) return;
            service.LoadRewarded();
        }

        /// <summary>
        /// Hiện rewarded video. Trả false ngay khi user đã mua No Ads.
        /// </summary>
        /// <param name="placement">Placement name cho analytics</param>
        /// <param name="onResult">true = user xem hết, cho reward</param>
        public static void ShowRewarded(string placement = "default", Action<bool> onResult = null)
        {
            var service = GetService();
            if (service == null || service.AdsDisabled)
            {
                if (service == null)
                    Debug.LogWarning("[Ads] IAdsService chưa đăng ký.");
                onResult?.Invoke(false);
                return;
            }
            service.ShowRewarded(placement, onResult);
        }

        // ─── CONTROL ───

        /// <summary>Tắt ads (sau khi mua Remove Ads).</summary>
        public static void DisableAds()
        {
            if (AdsManager.Instance)
                AdsManager.Instance.DisableAds();
        }

        /// <summary>Bật lại ads.</summary>
        public static void EnableAds()
        {
            if (AdsManager.Instance)
                AdsManager.Instance.EnableAds();
        }

        /// <summary>Ads service đã init xong chưa.</summary>
        public static bool IsAdsReady()
        {
            return GetService()?.IsInitialized ?? false;
        }

        // ─── SMART INTERSTITIAL (frequency capping) ───

        private static int _interstitialCounter;

        /// <summary>
        /// Hiện interstitial có frequency capping.
        /// Chỉ hiện sau mỗi N lần gọi. Giảm spam cho user.
        /// 
        /// <code>
        /// // Gọi sau mỗi level — chỉ hiện mỗi 3 level
        /// AdsExtensions.ShowInterstitialWithFrequency("level_end", 3, () => LoadNextLevel());
        /// </code>
        /// </summary>
        /// <param name="placement">Placement name</param>
        /// <param name="frequency">Hiện mỗi N lần gọi</param>
        /// <param name="onClosed">Callback khi ad đóng hoặc skip</param>
        public static void ShowInterstitialWithFrequency(string placement, int frequency, Action onClosed = null)
        {
            _interstitialCounter++;

            if (_interstitialCounter >= frequency)
            {
                _interstitialCounter = 0;
                ShowInterstitial(placement, onClosed);
            }
            else
            {
                onClosed?.Invoke();
            }
        }

        /// <summary>Reset frequency counter (vd: khi start new session).</summary>
        public static void ResetFrequencyCounter()
        {
            _interstitialCounter = 0;
        }

        // ─── HELPER ───

        private static IAdsService GetService()
        {
            return AdsManager.Instance != null ? AdsManager.Instance.Service : null;
        }
    }
}
