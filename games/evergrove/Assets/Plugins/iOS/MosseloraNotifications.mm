// Mosselora garden notes (iOS, I275; docs/specs/proposals/notifications-2026-11-27.md).
// One local notification at most, at the hour the player chose, replaced every time the
// game closes. Apple's UserNotifications only: no server, no SDK, nothing leaves the device.
// The permission prompt is requested only after the player's own yes in the game.
#import <Foundation/Foundation.h>
#import <UserNotifications/UserNotifications.h>

static NSString* const kMosseloraNoteId = @"mosselora.garden-note";
static int gMosseloraPermission = -1; // -1 unknown, 0 denied, 1 allowed

extern "C" void _MosseloraNotesRequest(void)
{
    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
    [center requestAuthorizationWithOptions:(UNAuthorizationOptionAlert | UNAuthorizationOptionSound)
                          completionHandler:^(BOOL granted, NSError* error) {
        gMosseloraPermission = granted ? 1 : 0;
    }];
}

extern "C" int _MosseloraNotesPermission(void)
{
    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
    [center getNotificationSettingsWithCompletionHandler:^(UNNotificationSettings* settings) {
        gMosseloraPermission = settings.authorizationStatus == UNAuthorizationStatusAuthorized ||
                               settings.authorizationStatus == UNAuthorizationStatusProvisional ? 1
                             : settings.authorizationStatus == UNAuthorizationStatusDenied ? 0 : -1;
    }];
    return gMosseloraPermission;
}

// Replaces any pending note with this one, at the given seconds from now.
extern "C" bool _MosseloraNotesSchedule(const char* title, const char* body, double secondsFromNow, const char* target)
{
    if (title == NULL || body == NULL || secondsFromNow < 60) return false;
    UNMutableNotificationContent* content = [[UNMutableNotificationContent alloc] init];
    content.title = [NSString stringWithUTF8String:title];
    content.body = [NSString stringWithUTF8String:body];
    content.sound = [UNNotificationSound defaultSound];
    if (target != NULL) content.userInfo = @{ @"target": [NSString stringWithUTF8String:target] };
    UNTimeIntervalNotificationTrigger* trigger =
        [UNTimeIntervalNotificationTrigger triggerWithTimeInterval:secondsFromNow repeats:NO];
    UNNotificationRequest* request =
        [UNNotificationRequest requestWithIdentifier:kMosseloraNoteId content:content trigger:trigger];
    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
    [center removePendingNotificationRequestsWithIdentifiers:@[kMosseloraNoteId]];
    [center addNotificationRequest:request withCompletionHandler:nil];
    return true;
}

extern "C" void _MosseloraNotesCancel(void)
{
    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
    [center removePendingNotificationRequestsWithIdentifiers:@[kMosseloraNoteId]];
    [center removeDeliveredNotificationsWithIdentifiers:@[kMosseloraNoteId]];
}
