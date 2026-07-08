using System;

namespace DraftUtils.Ads
{
    /// <summary>
    /// Interface trung tâm cho Ads service.
    /// Hỗ trợ: Banner, Interstitial, Rewarded Video.
    /// 
    /// Dùng cùng ServiceLocator — swap implementation giữa
    /// Unity Ads, AdMob, AppLovin MAX mà không đổi game code.
    /// 
    /// <code>
    /// var ads = ServiceLocator.Get&lt;IAdsService&gt;();
    /// ads.ShowInterstitial("level_end");
    /// ads.ShowRewarded("double_reward", result => { if(result) GiveReward(); });
    /// </code>
    /// </summary>
    public interface IAdsService
    {
        /// <summary>SDK đã khởi tạo xong chưa.</summary>
        bool IsInitialized { get; }

        /// <summary>Ads có bị tắt không (vd: user mua Remove Ads).</summary>
        bool AdsDisabled { get; set; }

        /// <summary>
        /// Khởi tạo Ads SDK.
        /// </summary>
        /// <param name="onComplete">Callback khi init xong</param>
        void Initialize(Action<bool> onComplete = null);

        // ─── BANNER ───

        /// <summary>Hiện banner ad.</summary>
        /// <param name="position">Vị trí banner</param>
        void ShowBanner(AdBannerPosition position);

        /// <summary>Ẩn banner ad.</summary>
        void HideBanner();

        /// <summary>Destroy banner (giải phóng memory).</summary>
        void DestroyBanner();

        // ─── INTERSTITIAL ───

        /// <summary>Interstitial đã load sẵn chưa.</summary>
        bool IsInterstitialReady { get; }

        /// <summary>Load interstitial ad.</summary>
        void LoadInterstitial();

        /// <summary>
        /// Hiện interstitial ad.
        /// </summary>
        /// <param name="placement">Tên placement (analytics)</param>
        /// <param name="onClosed">Callback khi ad đóng</param>
        void ShowInterstitial(string placement = "default", Action onClosed = null);

        // ─── REWARDED ───

        /// <summary>Rewarded video đã load sẵn chưa.</summary>
        bool IsRewardedReady { get; }

        /// <summary>Load rewarded video ad.</summary>
        void LoadRewarded();

        /// <summary>
        /// Hiện rewarded video ad.
        /// </summary>
        /// <param name="placement">Tên placement (analytics)</param>
        /// <param name="onResult">Callback: true = user xem hết, được reward</param>
        void ShowRewarded(string placement = "default", Action<bool> onResult = null);

        // ─── EVENTS ───

        /// <summary>Event khi ad hiện thành công (dùng cho analytics).</summary>
        event Action<AdEventInfo> OnAdShown;

        /// <summary>Event khi ad bị đóng.</summary>
        event Action<AdEventInfo> OnAdClosed;

        /// <summary>Event khi ad load thất bại.</summary>
        event Action<AdEventInfo> OnAdFailed;

        /// <summary>Event khi user nhận reward từ rewarded ad.</summary>
        event Action<AdEventInfo> OnRewardEarned;
    }
}
