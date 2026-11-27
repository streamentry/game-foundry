// Mosselora haptics (iOS, checklist 29 / I140 / EG-013). Three cues, one per
// meaning: a light tap when a move lands, a success pulse when a board or thing
// is finished, and a soft tap when something arrives. UIFeedbackGenerator honours
// the system's own haptics setting; the game calls these only while its own
// haptics switch is on. No data, no network.
#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator* gMosseloraPlaced = nil;

extern "C" void _MosseloraHapticPlaced(void)
{
    if (gMosseloraPlaced == nil)
        gMosseloraPlaced = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
    [gMosseloraPlaced impactOccurred];
    [gMosseloraPlaced prepare];
}

extern "C" void _MosseloraHapticFinished(void)
{
    UINotificationFeedbackGenerator* success = [[UINotificationFeedbackGenerator alloc] init];
    [success notificationOccurred:UINotificationFeedbackTypeSuccess];
}

extern "C" void _MosseloraHapticArrived(void)
{
    UIImpactFeedbackGenerator* soft = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
    [soft impactOccurred];
}
