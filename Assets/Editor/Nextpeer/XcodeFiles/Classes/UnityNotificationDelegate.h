#ifndef Unity_iPhone_UnityNotificationDelegate_h
#define Unity_iPhone_UnityNotificationDelegate_h
#import "Nextpeer/Nextpeer.h"
#import "NPBindings.h"

@interface UnityNotificationDelegate : NSObject<NPNotificationDelegate>

- (_NPTournamentStatusInfo) consumeTournamentStatusInfoWithId:(const char*)statusId;

@end
#endif
