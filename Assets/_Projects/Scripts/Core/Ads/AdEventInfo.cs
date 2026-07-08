namespace DraftUtils.Ads
{

    /// <summary>
    /// Thông tin event quảng cáo — dùng cho analytics và callbacks.
    /// </summary>
    public class AdEventInfo
    {
        /// <summary>Loại ad.</summary>
        public AdType AdType { get; set; }

        /// <summary>Placement name (vd: "level_end", "double_reward").</summary>
        public string Placement { get; set; }

        /// <summary>SDK đang dùng (vd: "UnityAds", "AdMob", "MAX").</summary>
        public string AdNetwork { get; set; }

        /// <summary>Error message nếu failed.</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Revenue (nếu có, từ impression-level ad revenue).</summary>
        public double Revenue { get; set; }

        /// <summary>Currency của revenue (vd: "USD").</summary>
        public string RevenueCurrency { get; set; }

        public AdEventInfo() { }

        public AdEventInfo(AdType type, string placement, string network)
        {
            AdType = type;
            Placement = placement;
            AdNetwork = network;
        }
    }
}
