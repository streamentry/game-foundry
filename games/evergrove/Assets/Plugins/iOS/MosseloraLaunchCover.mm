// Mosselora launch cover (iOS). Unity 6's trampoline shows a black Metal view
// until the first frame is rendered. Right after launch, this places a
// parchment window with the painted launch image above Unity; the game calls
// _MosseloraLaunchCoverDone() once its first screen is laid out, and the cover
// fades away. No network, no data, display only.
#import <UIKit/UIKit.h>

static UIWindow* gMosseloraCover = nil;

@interface MosseloraLaunchCover : NSObject
@end

@implementation MosseloraLaunchCover

+ (void)load
{
    [[NSNotificationCenter defaultCenter]
        addObserverForName: UIApplicationDidFinishLaunchingNotification
                    object: nil
                     queue: [NSOperationQueue mainQueue]
                usingBlock: ^(NSNotification* note) { [MosseloraLaunchCover show]; }];
}

+ (void)show
{
    if (gMosseloraCover != nil) return;
    UIWindow* cover = nil;
    if (@available(iOS 13.0, *))
    {
        for (UIScene* scene in UIApplication.sharedApplication.connectedScenes)
        {
            if ([scene isKindOfClass: [UIWindowScene class]])
            {
                cover = [[UIWindow alloc] initWithWindowScene: (UIWindowScene*)scene];
                break;
            }
        }
    }
    if (cover == nil) cover = [[UIWindow alloc] initWithFrame: UIScreen.mainScreen.bounds];
    cover.frame = UIScreen.mainScreen.bounds;
    cover.windowLevel = UIWindowLevelNormal + 1;
    cover.userInteractionEnabled = NO;
    // Page parchment #F9F2E3.
    UIColor* paper = [UIColor colorWithRed: 0.9765 green: 0.9490 blue: 0.8902 alpha: 1.0];
    UIViewController* host = [[UIViewController alloc] init];
    host.view.backgroundColor = paper;
    UIImage* art = [UIImage imageNamed: @"LaunchScreen-iPhonePortrait.png"];
    if (art != nil)
    {
        UIImageView* picture = [[UIImageView alloc] initWithImage: art];
        picture.frame = host.view.bounds;
        picture.contentMode = UIViewContentModeScaleAspectFill;
        picture.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
        [host.view addSubview: picture];
    }
    cover.rootViewController = host;
    cover.hidden = NO;
    gMosseloraCover = cover;
    // Safety net: never cover the game for more than 8 s, even if the game
    // never signals (for example a crash before the first screen).
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(8.0 * NSEC_PER_SEC)),
                   dispatch_get_main_queue(), ^{ [MosseloraLaunchCover hide]; });
}

+ (void)hide
{
    UIWindow* cover = gMosseloraCover;
    if (cover == nil) return;
    gMosseloraCover = nil;
    [UIView animateWithDuration: 0.25
                     animations: ^{ cover.alpha = 0.0; }
                     completion: ^(BOOL finished) { cover.hidden = YES; }];
}

@end

extern "C" void _MosseloraLaunchCoverDone()
{
    dispatch_async(dispatch_get_main_queue(), ^{ [MosseloraLaunchCover hide]; });
}
