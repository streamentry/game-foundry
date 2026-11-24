// EG-006 iteration 7: native text rendering prototype, iOS side.
// Renders one string with CoreText (the OS shaper, which forms Bengali conjuncts)
// into an RGBA bitmap with a transparent background. Uses only the system font
// cascade: no bundled fonts, no network, no file writes, no global state.
// Callers own the buffer. Output rows are bottom-up and colours un-premultiplied,
// which is the layout Unity's Texture2D.LoadRawTextureData expects.
#import <Foundation/Foundation.h>
#import <CoreText/CoreText.h>
#import <CoreGraphics/CoreGraphics.h>
#include <math.h>

static const CGFloat kPad = 2.0; // pixels

// align: 0 left, 1 centre, 2 right. bold: 0 regular, 1 bold.
static CTFontRef MosseloraFont(CGFloat size, int bold)
{
    // Language-tagged system UI font: CoreText cascades to a Bengali-capable face.
    CTFontRef base = CTFontCreateUIFontForLanguage(kCTFontUIFontSystem, size, CFSTR("bn"));
    if (base == NULL) return NULL;
    if (!bold) return base;
    CTFontRef heavy = CTFontCreateCopyWithSymbolicTraits(base, size, NULL, kCTFontTraitBold, kCTFontTraitBold);
    if (heavy == NULL) return base;
    CFRelease(base);
    return heavy;
}

static CTFramesetterRef MosseloraFramesetter(const char *utf8, CTFontRef font, CGColorRef ink, int align)
{
    NSString *text = [NSString stringWithUTF8String:utf8];
    if (text == nil) text = @"";
    CTTextAlignment ctAlign = kCTTextAlignmentLeft;
    if (align == 1) ctAlign = kCTTextAlignmentCenter;
    if (align == 2) ctAlign = kCTTextAlignmentRight;
    CTParagraphStyleSetting setting = { kCTParagraphStyleSpecifierAlignment, sizeof(CTTextAlignment), &ctAlign };
    CTParagraphStyleRef paragraph = CTParagraphStyleCreate(&setting, 1);
    NSDictionary *attrs = @{
        (__bridge id)kCTFontAttributeName: (__bridge id)font,
        (__bridge id)kCTForegroundColorAttributeName: (__bridge id)ink,
        (__bridge id)kCTParagraphStyleAttributeName: (__bridge id)paragraph
    };
    NSAttributedString *attributed = [[NSAttributedString alloc] initWithString:text attributes:attrs];
    CTFramesetterRef framesetter = CTFramesetterCreateWithAttributedString((__bridge CFAttributedStringRef)attributed);
    CFRelease(paragraph);
    return framesetter;
}

// outWH[0] = requested wrap width in pixels, outWH[1] = wrapped height in pixels.
extern "C" void MosseloraNativeTextMeasure(const char *utf8, float size, float maxWidth, int bold, int *outWH)
{
    outWH[0] = (int)ceil(maxWidth);
    outWH[1] = 0;
    CTFontRef font = MosseloraFont(size, bold);
    if (font == NULL) return;
    CGColorRef ink = CGColorCreateGenericRGB(0, 0, 0, 1);
    CTFramesetterRef framesetter = MosseloraFramesetter(utf8, font, ink, 0);
    CGSize fit = CTFramesetterSuggestFrameSizeWithConstraints(framesetter, CFRangeMake(0, 0), NULL,
                                                              CGSizeMake(maxWidth - 2 * kPad, CGFLOAT_MAX), NULL);
    outWH[1] = (int)ceil(fit.height) + (int)(2 * kPad);
    CFRelease(framesetter);
    CGColorRelease(ink);
    CFRelease(font);
}

// Writes w*h*4 bytes of RGBA, bottom row first, straight (un-premultiplied) alpha.
extern "C" void MosseloraNativeTextRender(const char *utf8, float size, float maxWidth, int bold, int align,
                                          float r, float g, float b, float a, unsigned char *buffer, int w, int h)
{
    if (buffer == NULL || w <= 0 || h <= 0) return;
    CGColorSpaceRef space = CGColorSpaceCreateDeviceRGB();
    CGContextRef ctx = CGBitmapContextCreate(buffer, w, h, 8, (size_t)w * 4, space,
                                             kCGImageAlphaPremultipliedLast);
    CGColorSpaceRelease(space);
    if (ctx == NULL) return;
    CGContextClearRect(ctx, CGRectMake(0, 0, w, h));

    CTFontRef font = MosseloraFont(size, bold);
    if (font != NULL) {
        CGColorRef ink = CGColorCreateGenericRGB(r, g, b, a);
        CTFramesetterRef framesetter = MosseloraFramesetter(utf8, font, ink, align);
        CGPathRef path = CGPathCreateWithRect(CGRectMake(kPad, kPad, w - 2 * kPad, h - 2 * kPad), NULL);
        CTFrameRef frame = CTFramesetterCreateFrame(framesetter, CFRangeMake(0, 0), path, NULL);
        CTFrameDraw(frame, ctx);
        CFRelease(frame);
        CGPathRelease(path);
        CFRelease(framesetter);
        CGColorRelease(ink);
        CFRelease(font);
    }
    CGContextRelease(ctx);

    // CoreText memory is top row first; Unity expects bottom row first.
    // Swap rows, then convert premultiplied colour to straight alpha.
    const int rowBytes = w * 4;
    for (int y = 0; y < h / 2; y++) {
        unsigned char *top = buffer + (size_t)y * rowBytes;
        unsigned char *bottom = buffer + (size_t)(h - 1 - y) * rowBytes;
        for (int x = 0; x < rowBytes; x++) {
            unsigned char tmp = top[x];
            top[x] = bottom[x];
            bottom[x] = tmp;
        }
    }
    for (int i = 0; i < w * h * 4; i += 4) {
        unsigned int a = buffer[i + 3];
        if (a == 0) {
            buffer[i] = buffer[i + 1] = buffer[i + 2] = 0;
        } else if (a < 255) {
            for (int c = 0; c < 3; c++) {
                unsigned int v = ((unsigned int)buffer[i + c] * 255u + a / 2) / a;
                buffer[i + c] = (unsigned char)(v > 255u ? 255u : v);
            }
        }
    }
}
