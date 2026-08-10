using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DraftUtils.Ads.Editor
{
    /// <summary>
    /// Giữ AdConfigSO là nguồn App ID duy nhất và chặn build production
    /// nếu Ad Unit ID còn thiếu, sai định dạng hoặc vẫn là ID test của Google.
    /// </summary>
    internal sealed class AdMobProductionConfigBuildValidator : IPreprocessBuildWithReport
    {
        private const string ConfigPath =
            "Assets/_Projects/Scripts/Core/Ads/ScriptableObjects/AdMob_AdsConfig.asset";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            var config = LoadConfig();
            if (config.SdkType != AdSDKType.AdMob)
            {
                return;
            }

            var platformConfig = GetPlatformConfig(config, report.summary.platform);
            if (platformConfig == null)
            {
                return;
            }

            if (!platformConfig.HasValidAdMobAppId)
            {
                throw new BuildFailedException(
                    "AdMob App ID chưa hợp lệ. Mở MyMenu > Monetization > AdMob Production IDs.");
            }

            if (!config.TestMode && !platformConfig.HasValidAdMobProductionIds)
            {
                throw new BuildFailedException(
                    "AdMob production IDs chưa đầy đủ hoặc vẫn là ID test của Google. " +
                    "Cần App ID, Banner, Interstitial và Rewarded ID thật.");
            }

            string syncMessage;
            if (!config.TrySyncGoogleMobileAdsAppIds(out syncMessage))
            {
                throw new BuildFailedException(syncMessage);
            }

            if (config.TestMode)
            {
                Debug.LogWarning(
                    "[Ads Config] Build đang bật Test Mode; Ad Unit ID test của Google sẽ được sử dụng.");
            }
        }

        [MenuItem("MyMenu/Monetization/AdMob Production IDs", false, 220)]
        private static void SelectProductionConfig()
        {
            var config = LoadConfig();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        [MenuItem("MyMenu/Monetization/Validate AdMob Production IDs", false, 221)]
        private static void ValidateProductionConfig()
        {
            var config = LoadConfig();
            if (config.SdkType != AdSDKType.AdMob)
            {
                Debug.LogWarning("[Ads Config] Ads SDK hiện không phải AdMob.", config);
                return;
            }

            var platformConfig = GetPlatformConfig(config, EditorUserBuildSettings.activeBuildTarget);
            if (platformConfig == null)
            {
                Debug.LogWarning("[Ads Config] Chỉ kiểm tra production ID cho Android/iOS.", config);
                return;
            }

            if (!platformConfig.HasValidAdMobAppId)
            {
                Debug.LogError("[Ads Config] App ID chưa hợp lệ.", config);
                return;
            }

            if (!config.TestMode && !platformConfig.HasValidAdMobProductionIds)
            {
                Debug.LogError(
                    "[Ads Config] Production IDs chưa đầy đủ hoặc vẫn là ID test của Google.", config);
                return;
            }

            string syncMessage;
            if (!config.TrySyncGoogleMobileAdsAppIds(out syncMessage))
            {
                Debug.LogError("[Ads Config] " + syncMessage, config);
                return;
            }

            Debug.Log(
                config.TestMode
                    ? "[Ads Config] Hợp lệ, nhưng Test Mode vẫn đang bật."
                    : "[Ads Config] Production IDs hợp lệ và App ID đã được đồng bộ.",
                config);
        }

        private static AdConfigSO LoadConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<AdConfigSO>(ConfigPath);
            if (config == null)
            {
                throw new BuildFailedException("Không tìm thấy AdMob_AdsConfig.asset tại " + ConfigPath);
            }

            return config;
        }

        private static PlatformAdsConfig GetPlatformConfig(AdConfigSO config, BuildTarget target)
        {
            if (target == BuildTarget.Android)
            {
                return config.androidConfig;
            }

            if (target == BuildTarget.iOS)
            {
                return config.iosConfig;
            }

            return null;
        }
    }
}
