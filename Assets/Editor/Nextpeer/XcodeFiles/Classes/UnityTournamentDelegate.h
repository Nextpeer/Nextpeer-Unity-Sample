#ifndef Unity_iPhone_UnityTournamentDelegate_h
#define Unity_iPhone_UnityTournamentDelegate_h
#import "Nextpeer/Nextpeer.h"
#import "NPBindings.h"

@interface UnityTournamentDelegate : NSObject<NPTournamentDelegate>
{
}

// Tournament custom message accessor
-(BOOL)consumeCustomMessageWithId:(const char*)MessageID message:(_NPTournamentCustomMessageContainer*)message;

-(BOOL)consumeSyncEventWithId:(const char*)infoObjectId infoObject:(_NPSyncEventInfo*)syncEventInfo;


-(void)nextpeerDidReceiveTournamentCustomMessage:(NPTournamentCustomMessageContainer*)message;
-(void)nextpeerDidReceiveUnreliableTournamentCustomMessage:(NPTournamentCustomUnreliableMessageContainer*)message;
- (_NPTournamentStatusInfo) consumeTournamentStatusInfoWithId:(const char*)statusId;

-(_NPTournamentStatusInfo) consumeTournamentStatusInfoWithId:(const char*)statusId;

@end

#endif
