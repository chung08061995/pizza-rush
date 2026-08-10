using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils.Ads
{
    /// <summary>
    /// Cấu hình quảng cáo cho một nền tảng (Android/iOS).
    /// App ID dùng dấu ~; các Ad Unit ID dùng dấu /.
    /// </summary>
    [System.Serializable]
    public class PlatformAdsConfig
    {
        private const string GoogleTestPublisherPrefix = "ca-app-pub-3940256099942544";

        [LabelText("AdMob App ID (~)")]
        [Tooltip("App ID, ví dụ ca-app-pub-...~...")]
        [ValidateInput(nameof(IsValidAppIdOrEmpty), "App ID phải có dạng ca-app-pub-...~...")]
        public string appId = "";

        [LabelText("Banner Ad Unit ID (/)")]
        [Tooltip("Ad Unit ID của Banner. Để trống nếu không sử dụng.")]
        [ValidateInput(nameof(IsValidAdUnitIdOrEmpty), "Ad Unit ID phải có dạng ca-app-pub-.../...")]
        public string bannerId = "";

        [LabelText("Interstitial Ad Unit ID (/)")]
        [Tooltip("Ad Unit ID của Interstitial.")]
        [ValidateInput(nameof(IsValidAdUnitIdOrEmpty), "Ad Unit ID phải có dạng ca-app-pub-.../...")]
        public string interstitialId = "";

        [LabelText("Rewarded Ad Unit ID (/)")]
        [Tooltip("Ad Unit ID của Rewarded Video.")]
        [ValidateInput(nameof(IsValidAdUnitIdOrEmpty), "Ad Unit ID phải có dạng ca-app-pub-.../...")]
        public string rewardedId = "";

        public bool HasValidAdMobAppId => IsAdMobAppId(appId);

        public bool HasProductionAdMobAppId =>
            HasValidAdMobAppId && !IsGoogleTestId(appId);

        public bool HasValidAdMobProductionIds =>
            HasProductionAdMobAppId &&
            IsAdMobAdUnitId(bannerId) && !IsGoogleTestId(bannerId) &&
            IsAdMobAdUnitId(interstitialId) && !IsGoogleTestId(interstitialId) &&
            IsAdMobAdUnitId(rewardedId) && !IsGoogleTestId(rewardedId);


        public static bool IsAdMobAppId(string value)
        {
            return HasPrefix(value) && value.Contains("~") && !value.Contains("/");
        }

        public static bool IsAdMobAdUnitId(string value)
        {
            return HasPrefix(value) && value.Contains("/") && !value.Contains("~");
        }

        public static bool IsGoogleTestId(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Trim().StartsWith(GoogleTestPublisherPrefix, System.StringComparison.Ordinal);
        }

        private bool IsValidAppIdOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) || IsAdMobAppId(value);
        }

        private bool IsValidAdUnitIdOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) || IsAdMobAdUnitId(value);
        }

        private static bool HasPrefix(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Trim().StartsWith("ca-app-pub-", System.StringComparison.Ordinal);
        }
    }
}
