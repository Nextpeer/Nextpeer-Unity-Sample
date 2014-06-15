#import "NPDynamicDefines.h"

#ifdef UNITY_4_2_APP_CONTORLLER_STYLE
#import "UnityAppController.h"
#else
#import "AppController.h"
#endif

#import <UIKit/UIKit.h>
#import "Nextpeer/Nextpeer.h"
#include "UnityNextPeerDelegate.h"
#include "UnityTournamentDelegate.h"
#include "UnityNotificationDelegate.h"
#include "UnityCurrencyDelegate.h"

@interface NextpeerAppController :
#ifdef UNITY_4_2_APP_CONTORLLER_STYLE
UnityAppController
#else
AppController
#endif


// DelegatesHandler accessor for bindings
+ (UnityNextpeerDelegate*) GetNextpeerDelegate;
+ (UnityTournamentDelegate*) GetTournamentDelegate;
+ (UnityNotificationDelegate*) GetNotificationDelegate;
+ (UnityCurrencyDelegate*) GetCurrencyDelegate;

+(void)MarkNextpeerAsInitialised;

@end
