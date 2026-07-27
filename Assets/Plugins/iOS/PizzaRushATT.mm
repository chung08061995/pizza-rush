#import <AppTrackingTransparency/AppTrackingTransparency.h>

typedef void (*PizzaRushATTCallback)(int status);

extern "C" void PizzaRushRequestTrackingAuthorization(PizzaRushATTCallback callback)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 14.0, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                if (callback) callback((int)status);
            }];
        } else if (callback) {
            callback(3);
        }
    });
}
