#include "UnityCurrencyDelegate.h"
#include "NPConstants.h"
#include "iPhone_target_Prefix.pch"

static NSUInteger CurrencyAmount = 0;
static BOOL SupportUnifiedCurrency = YES;

@implementation UnityCurrencyDelegate

// Unity GameObjectPath Setter/getter
//------------------------------------------------

// CurrencyAmount accessors
- (NSInteger) getCurrencyAmount
{
    return CurrencyAmount;
}

- (void)setCurrencyAmount:(NSInteger) amount
{
    CurrencyAmount = amount;
}

// Currency Support accessors
- (BOOL) isUnifiedCurrencySupported
{
    return SupportUnifiedCurrency;
}

- (void) switchUnifiedCurrencySupported:(BOOL) isSupported
{
    SupportUnifiedCurrency = isSupported;
}

- (NSUInteger)nextpeerGetCurrency
{
    return CurrencyAmount;
}

- (void)nextpeerAddAmountToCurrency:(NSInteger)amount
{
    CurrencyAmount += amount;
    
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_ADD_AMOUNT_TO_CURRENCY,
                     [[NSString stringWithFormat:@"%d", amount] UTF8String]);
}

- (BOOL)nextpeerSupportsUnifiedCurrency
{
    return SupportUnifiedCurrency;
}

@end