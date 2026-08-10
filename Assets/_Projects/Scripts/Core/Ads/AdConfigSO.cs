using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DraftUtils.Ads
{
    /// <summary>
    /// Nguồn cấu hình duy nhất cho SDK quảng cáo và các production ID.
    /// TestMode dùng Ad Unit ID mẫu của Google; khi tắt sẽ dùng ID bên dưới.
    /// </summary>
    [CreateAssetMenu(fileName = "AdsConfig", menuName = "DraftUtils/" + nameof(AdConfigSO))]
    public class AdConfigSO : ScriptableObject
    {
#if UNITY_EDITOR
        private const string GoogleMobileAdsSettingsPath =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
#endif

        [TitleGroup("Runtime Mode")]
        [LabelText("Ads SDK")]
        public AdSDKType SdkType = AdSDKType.None;

        [TitleGroup("Runtime Mode")]
        [ToggleLeft]
        [LabelText("Use Google Test Ad Unit IDs")]
        [InfoBox("Test Mode đang bật: Banner, Interstitial và Rewarded sẽ dùng ID test của Google.", InfoMessageType.Warning, nameof(TestMode))]
        public bool TestMode = true;

        [TitleGroup("Production IDs")]
        [InfoBox("Nhập App ID có dấu ~ và các Ad Unit ID có dấu /. Sau khi nhập xong, tắt Test Mode để build production.")]
        [InfoBox("Android production IDs đang thiếu, sai định dạng hoặc vẫn là ID test của Google.", InfoMessageType.Error, nameof(HasInvalidAndroidProductionIds))]
        [BoxGroup("Production IDs/Android")]
        [InlineProperty, HideLabel]
        public PlatformAdsConfig androidConfig = new();

        [BoxGroup("Production IDs/iOS (Optional)")]
        [InlineProperty, HideLabel]
        public PlatformAdsConfig iosConfig = new();

        [Header("AppLovin MAX (Optional)")]
        [Tooltip("MAX SDK Key (only for AppLovin)")]
        [ShowIf(nameof(SdkType), AdSDKType.AppLovinMAX)]
        public string MaxSdkKey = "";

        public PlatformAdsConfig platformConfig
        {
            get
            {
                return DraftUtils.Utils.PlatformExecuteUtils.GetValuePlatform(
                    androidConfig, iosConfig, androidConfig, new PlatformAdsConfig());
            }
        }

        public bool HasValidAndroidProductionIds =>
            androidConfig != null && androidConfig.HasValidAdMobProductionIds;

        // Odin evaluates the InfoBox condition while compiling player assemblies too,
        // so this member must remain available outside UNITY_EDITOR.
        private bool HasInvalidAndroidProductionIds =>
            SdkType == AdSDKType.AdMob && !TestMode && !HasValidAndroidProductionIds;

#if UNITY_EDITOR
        [TitleGroup("Production IDs")]
        [Button("Apply App IDs to Google Mobile Ads Settings", ButtonSizes.Large)]
        [EnableIf(nameof(CanSyncAdMobAppIds))]
        private void ApplyAdMobAppIdsToPluginSettings()
        {
            string message;
            if (TrySyncGoogleMobileAdsAppIds(out message))
            {
                Debug.Log($"[Ads Config] {message}", this);
            }
            else
            {
                Debug.LogError($"[Ads Config] {message}", this);
            }
        }

        public bool TrySyncGoogleMobileAdsAppIds(out string message)
        {
            if (SdkType != AdSDKType.AdMob)
            {
                message = "Ads SDK phải là AdMob.";
                return false;
            }

            if (androidConfig == null || !androidConfig.HasValidAdMobAppId)
            {
                message = "Android App ID phải có dạng ca-app-pub-...~...";
                return false;
            }

            var settings = AssetDatabase.LoadMainAssetAtPath(GoogleMobileAdsSettingsPath);
            if (settings == null)
            {
                message = "Không tìm thấy GoogleMobileAdsSettings.asset.";
                return false;
            }

            var serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            var androidAppId = serializedSettings.FindProperty("adMobAndroidAppId");
            var iosAppId = serializedSettings.FindProperty("adMobIOSAppId");
            if (androidAppId == null || iosAppId == null)
            {
                message = "Google Mobile Ads Settings không có trường App ID mong đợi.";
                return false;
            }

            androidAppId.stringValue = androidConfig.appId.Trim();
            if (iosConfig != null && iosConfig.HasValidAdMobAppId)
            {
                iosAppId.stringValue = iosConfig.appId.Trim();
            }

            serializedSettings.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            message = "Đã đồng bộ App ID sang Google Mobile Ads Settings.";
            return true;
        }

        private bool IsAdMob => SdkType == AdSDKType.AdMob;
        private bool CanSyncAdMobAppIds =>
            IsAdMob && androidConfig != null && androidConfig.HasValidAdMobAppId;
#endif
    }
}
