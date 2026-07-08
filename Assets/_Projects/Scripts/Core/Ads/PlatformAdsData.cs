namespace DraftUtils.Ads
{

    /// <summary>
    /// Cấu hình quảng cáo cho một nền tảng (Android/iOS).
    /// Bao gồm App ID và các Ad Unit ID được sử dụng bởi SDK quảng cáo.
    /// </summary>
    [System.Serializable]
    public class PlatformAdsConfig
    {
        /// <summary>
        /// App ID của SDK quảng cáo (ví dụ: AdMob App ID).
        /// </summary>
        public string appId;

        /// <summary>
        /// Ad Unit ID của quảng cáo Banner.
        /// Để trống nếu không sử dụng.
        /// </summary>
        public string bannerId = "";

        /// <summary>
        /// Ad Unit ID của quảng cáo Interstitial.
        /// Để trống nếu không sử dụng.
        /// </summary>
        public string interstitialId = "";

        /// <summary>
        /// Ad Unit ID của quảng cáo Rewarded.
        /// Để trống nếu không sử dụng.
        /// </summary>
        public string rewardedId = "";
    }
}