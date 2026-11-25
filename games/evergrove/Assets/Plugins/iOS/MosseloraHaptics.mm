// Mosselora haptics (iOS, I140 / EG-013). A light tick when a tile turns and a
// success pulse when a stream is finished. UIFeedbackGenerator honours the
// system's own haptics setting; the game calls these only while gentle motion
// is on. No data, no network.
#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator* gMosseloraTick = nil;

extern "C" void _MosseloraHapticTick(void)
{
    if (gMosseloraTick == nil)
        gMosseloraTick = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
    [gMosseloraTick impactOccurred];
    [gMosseloraTick prepare];
}

extern "C" void _MosseloraHapticSuccess(void)
{
    UINotificationFeedbackGenerator* success = [[UINotificationFeedbackGenerator alloc] init];
    [success notificationOccurred:UINotificationFeedbackTypeSuccess];
}
