using System;
using System.Runtime.InteropServices;

namespace DraftUtils.Ads
{
    internal static class ATTAuthorization
    {
#if UNITY_IOS && !UNITY_EDITOR
        private delegate void NativeCallback(int status);
        private static Action _completed;

        [DllImport("__Internal")]
        private static extern void PizzaRushRequestTrackingAuthorization(NativeCallback callback);

        public static void Request(Action completed)
        {
            _completed = completed;
            PizzaRushRequestTrackingAuthorization(OnNativeCompleted);
        }

        [AOT.MonoPInvokeCallback(typeof(NativeCallback))]
        private static void OnNativeCompleted(int status)
        {
            var callback = _completed;
            _completed = null;
            callback?.Invoke();
        }
#else
        public static void Request(Action completed) => completed?.Invoke();
#endif
    }
}
