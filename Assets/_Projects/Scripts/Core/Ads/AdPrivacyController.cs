using System;
using UnityEngine;

#if GOOGLE_ADMOB
using GoogleMobileAds.Ump.Api;
#endif

namespace DraftUtils.Ads
{
    /// <summary>Centralized UMP consent gate. Ads never block the game when consent is unavailable.</summary>
    public static class AdPrivacyController
    {
        private static bool _requested;
        private static bool _canRequest;

        public static bool CanRequestAds =>
#if GOOGLE_ADMOB
            _requested ? _canRequest : ConsentInformation.CanRequestAds();
#else
            true;
#endif

        public static bool PrivacyOptionsRequired =>
#if GOOGLE_ADMOB
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
#else
            false;
#endif

        public static void RequestConsent(Action<bool> completed = null)
        {
#if GOOGLE_ADMOB
            if (_requested) { completed?.Invoke(_canRequest); return; }
            var parameters = new ConsentRequestParameters { TagForUnderAgeOfConsent = false };
            ConsentInformation.Update(parameters, error =>
            {
                if (error != null) Debug.LogWarning("[Ads] UMP update failed: " + error.Message);
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning("[Ads] UMP form failed: " + formError.Message);
                    _canRequest = ConsentInformation.CanRequestAds();
                    ATTAuthorization.Request(() =>
                    {
                        _requested = true;
                        completed?.Invoke(_canRequest);
                    });
                });
            });
#else
            _requested = true;
            _canRequest = true;
            completed?.Invoke(true);
#endif
        }

        public static void ShowPrivacyOptions(Action<bool> completed = null)
        {
#if GOOGLE_ADMOB
            ConsentForm.ShowPrivacyOptionsForm(error =>
            {
                if (error != null) Debug.LogWarning("[Ads] Privacy options failed: " + error.Message);
                _canRequest = ConsentInformation.CanRequestAds();
                completed?.Invoke(_canRequest);
            });
#else
            completed?.Invoke(false);
#endif
        }
    }
}
