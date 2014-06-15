
#import "NextpeerAppController.h"

#define GAME_KEY @"Game key"

// Nextpeer Delegates Handler delegates (mainly used for
// external accessors which have to be pInvoked)
static UnityNextpeerDelegate* NextpeerDelegate = nil;
static UnityTournamentDelegate* TournamentDelegate = nil;
static UnityNotificationDelegate* NotificationDelegate = nil;
static UnityCurrencyDelegate* CurrencyDelegate = nil;

// Memorise Launch options for handling notifications
static NSDictionary* NPLaunchOptions = nil;
static BOOL IsNextpeerInitialised = NO;

@interface NextpeerAppController ()

+(void)HandleLaunchOptions;

@end

@implementation NextpeerAppController

+ (UnityNextpeerDelegate*) GetNextpeerDelegate
{
    return NextpeerDelegate;
}

+ (UnityTournamentDelegate*) GetTournamentDelegate
{
    return TournamentDelegate;
}

+ (UnityNotificationDelegate*) GetNotificationDelegate
{
    return NotificationDelegate;
}

+ (UnityCurrencyDelegate*) GetCurrencyDelegate
{
    return CurrencyDelegate;
}

+(void)MarkNextpeerAsInitialised
{
    IsNextpeerInitialised = YES;
    [self HandleLaunchOptions];
}

+(void)HandleLaunchOptions
{
    if (!IsNextpeerInitialised || NPLaunchOptions == nil)
    {
        return;
    }
    
    [Nextpeer handleLaunchOptions:NPLaunchOptions];
    [NPLaunchOptions release];
    NPLaunchOptions = nil;
}

- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification
{
    [Nextpeer handleLocalNotification:notification];
    
    [super application:application didReceiveLocalNotification:notification];
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    [Nextpeer handleRemoteNotification:userInfo];
    
    [super application:application didReceiveRemoteNotification:userInfo];
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
    [Nextpeer registerDeviceToken:deviceToken];
    
    [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}

-(BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    BOOL superResult = [super application:application didFinishLaunchingWithOptions:launchOptions];
    
    static BOOL delegatesCreated = NO;
    if (!delegatesCreated)
    {
        // Nextpeer initialisation with custom Handler provided
        NextpeerDelegate = [[UnityNextpeerDelegate alloc] init];
        TournamentDelegate =[[UnityTournamentDelegate  alloc] init];
        NotificationDelegate = [[UnityNotificationDelegate alloc] init];
        CurrencyDelegate = [[UnityCurrencyDelegate alloc] init];
        
        delegatesCreated = YES;
    }
    
    if (NPLaunchOptions != nil)
    {
        [NPLaunchOptions release];
    }
    
    NPLaunchOptions = [launchOptions retain];
    
    [NextpeerAppController HandleLaunchOptions];
    
    return superResult;
}

-(BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    if ([Nextpeer handleOpenURL:url]) {
        return YES;
    }
    
    return [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
}

@end
