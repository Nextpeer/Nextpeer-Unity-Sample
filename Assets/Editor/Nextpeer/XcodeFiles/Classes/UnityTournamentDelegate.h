#ifndef Unity_iPhone_UnityTournamentDelegate_h
#define Unity_iPhone_UnityTournamentDelegate_h
#import "Nextpeer/Nextpeer.h"
#import "NPBindings.h"

@interface UnityTournamentDelegate : NSObject<NPTournamentDelegate>
{
    _NPTournamentEndDataContainer TournamentEnd;
}

// Tournament custom message accessor
-(BOOL)consumeCustomMessageWithId:(const char*)MessageID message:(_NPTournamentCustomMessageContainer*)message;

-(BOOL)consumeSyncEventWithId:(const char*)infoObjectId infoObject:(_NPSyncEventInfo*)syncEventInfo;

// Tournament data access
- (_NPTournamentEndDataContainer) getTournamentEndDataContainer;

-(void)nextpeerDidReceiveTournamentCustomMessage:(NPTournamentCustomMessageContainer*)message;
-(void)nextpeerDidReceiveUnreliableTournamentCustomMessage:(NPTournamentCustomUnreliableMessageContainer*)message;
-(void)nextpeerDidReceiveTournamentResults:(NPTournamentEndDataContainer*)tournamentContainer;

@end

#endif
