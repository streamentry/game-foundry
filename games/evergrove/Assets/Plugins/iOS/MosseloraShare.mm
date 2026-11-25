// Mosselora share sheet (iOS, I149). Opens the system share sheet with the
// garden postcard text, so a player sends it the way they send anything else
// (Messages, Mail, WhatsApp). The game uploads nothing; the player chooses.
#import <UIKit/UIKit.h>

extern "C" UIViewController* UnityGetGLViewController();

extern "C" bool _MosseloraShareText(const char* text)
{
    if (text == NULL) return false;
    NSString* body = [NSString stringWithUTF8String:text];
    UIViewController* root = UnityGetGLViewController();
    if (body == nil || root == nil) return false;
    UIActivityViewController* sheet =
        [[UIActivityViewController alloc] initWithActivityItems:@[body] applicationActivities:nil];
    UIPopoverPresentationController* popover = sheet.popoverPresentationController;
    if (popover != nil)
    {
        // iPad: a centred popover with no arrow.
        popover.sourceView = root.view;
        popover.sourceRect = CGRectMake(CGRectGetMidX(root.view.bounds), CGRectGetMidY(root.view.bounds), 1, 1);
        popover.permittedArrowDirections = 0;
    }
    [root presentViewController:sheet animated:YES completion:nil];
    return true;
}
