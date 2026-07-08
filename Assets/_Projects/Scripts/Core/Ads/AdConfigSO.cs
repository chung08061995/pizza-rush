using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DraftUtils.Ads
{
    /// <summary>
    /// ScriptableObject chứa cấu hình Ad IDs cho từng platform.
    /// Tạo via: Create → DraftUtils → Ad Config
    /// 
    /// Dùng chung cho mọi SDK — chỉ cần set IDs tương ứng.
    /// </summary>
    [CreateAssetMenu(fileName = "AdsConfig", menuName = "DraftUtils/" + nameof(AdConfigSO))]
    public class AdConfigSO : ScriptableObject
    {
        public AdSDKType SdkType = AdSDKType.None;
        public bool TestMode = true;
        public PlatformAdsConfig androidConfig = new();
        public PlatformAdsConfig iosConfig = new();

        [Header("AppLovin MAX (Optional)")]
        [Tooltip("MAX SDK Key (only for AppLovin)")]
        [ShowIf(nameof(SdkType), AdSDKType.AppLovinMAX)]
        public string MaxSdkKey = "";

        // ─── Platform-aware getters ───
        public PlatformAdsConfig platformConfig
        {
            get
            {
                return DraftUtils.Utils.PlatformExecuteUtils.GetValuePlatform(androidConfig, iosConfig, androidConfig, new());
            }
        }
    }
}
