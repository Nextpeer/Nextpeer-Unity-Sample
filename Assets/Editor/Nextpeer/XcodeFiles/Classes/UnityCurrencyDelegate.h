#ifndef Unity_iPhone_UnityCurrencyDelegate_h
#define Unity_iPhone_UnityCurrencyDelegate_h
#import "Nextpeer/Nextpeer.h"

@interface UnityCurrencyDelegate : NSObject<NPCurrencyDelegate>

// Management of the currency Amount from Unity
- (NSInteger) getCurrencyAmount;
- (void)setCurrencyAmount:(NSInteger) amount;

// Unified currency support
- (BOOL) isUnifiedCurrencySupported;
- (void) switchUnifiedCurrencySupported:(BOOL) isSupported;

- (NSUInteger)nextpeerGetCurrency;
- (void)nextpeerAddAmountToCurrency:(NSInteger)amount;
- (BOOL)nextpeerSupportsUnifiedCurrency;

@end
#endif
