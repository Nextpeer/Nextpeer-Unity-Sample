#ifndef Unity_iPhone_UnityNextPeerDelegate_h
#define Unity_iPhone_UnityNextPeerDelegate_h
#import "Nextpeer/Nextpeer.h"
#include "NPBindings.h"


@interface UnityNextpeerDelegate : NSObject<NextpeerDelegate>

// Manage a list of accepted tournament Id(s) that is used in nextpeerSupportsTournamentWithId delegate
- (void)addWhitelistTournamentId:(NSString*) tournamentId;
- (void)removeWhitelistTournamentId:(NSString*) tournamentId;
- (void)clearTournamentWhitelist;

- (void)addBlacklistTournamentId:(NSString*) tournamentId;
- (void)removeBlacklistTournamentId:(NSString*) tournamentId;
- (void)clearTournamentBlacklist;

// Manage the Nextpeer Not supported should show errors flag
- (void) setNextpeerNotSupportedShouldShowErrors:(bool) ShouldShow;
- (BOOL) getNextpeerNotSupportedShouldShowErrors;

// Manage NPTournamentStartDataContainer
- (_NPTournamentStartDataContainer) getTournamentStartDataContainer;

- (BOOL)nextpeerSupportsTournamentWithId:(NSString* )tournamentUuid;
- (BOOL)nextpeerNotSupportedShouldShowCustomError;

// Delegates that sends messages to Unity
- (void)nextpeerDidTournamentStartWithDetails:(NPTournamentStartDataContainer *)tournamentContainer;
- (void)nextpeerDashboardWillAppear;
- (void)nextpeerDashboardDidAppear;
- (void)nextpeerDashboardWillDisappear;
- (void)nextpeerDashboardDidDisappear;
- (void)nextpeerDashboardDidReturnToGame;
- (void)nextpeerDidTournamentEnd;

@end
#endif
