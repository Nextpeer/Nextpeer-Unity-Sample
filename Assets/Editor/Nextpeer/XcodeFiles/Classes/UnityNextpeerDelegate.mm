#include "UnityNextpeerDelegate.h"
#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

#include "NPConstants.h"
#include "NextpeerAppController.h"
#import "NPTournamentObjectsContainer.h"

extern void UnitySendMessage(const char *, const char *, const char *);

static BOOL notSupportedShouldShowCustomError = NO;
static NSMutableSet* whitelistTournamentIds = [[NSMutableSet alloc] init];
static NSMutableSet* blacklistTournamentIds = [[NSMutableSet alloc] init];

@interface UnityNextpeerDelegate ()
{
    _NPTournamentStartDataContainer TournamentStart;
}

@end

@implementation UnityNextpeerDelegate

//------------------------------------------------
-(void)addWhitelistTournamentId:(NSString *)tournamentId
{
    [whitelistTournamentIds addObject:tournamentId];
}

-(void)removeWhitelistTournamentId:(NSString *)tournamentId
{
    [whitelistTournamentIds removeObject:tournamentId];
}

-(void)clearTournamentWhitelist
{
    [whitelistTournamentIds removeAllObjects];
}

-(void)addBlacklistTournamentId:(NSString *)tournamentId
{
    [blacklistTournamentIds addObject:tournamentId];
}

-(void)removeBlacklistTournamentId:(NSString *)tournamentId
{
    [blacklistTournamentIds removeObject:tournamentId];
}

-(void)clearTournamentBlacklist
{
    [blacklistTournamentIds removeAllObjects];
}

//------------------------------------------------
// NextpeerNotSupportedShouldShowCustomError management
- (void) setNextpeerNotSupportedShouldShowErrors:(bool) ShouldShow
{
    notSupportedShouldShowCustomError = ShouldShow;
}

- (BOOL) getNextpeerNotSupportedShouldShowErrors
{
    return  notSupportedShouldShowCustomError;
}
//------------------------------------------------
// NPTournamentStartDataContainer management
- (_NPTournamentStartDataContainer) getTournamentStartDataContainer
{    
    return TournamentStart;
}

//------------------------------------------------

- (BOOL) nextpeerSupportsTournamentWithId:(NSString* ) tournamentUuid
{
    if ([whitelistTournamentIds count] > 0)
    {
        return [whitelistTournamentIds containsObject:tournamentUuid];
    }
    if ([blacklistTournamentIds count] > 0)
    {
        return ![blacklistTournamentIds containsObject:tournamentUuid];
    }
    return YES;
}

- (BOOL)nextpeerNotSupportedShouldShowCustomError
{
   return notSupportedShouldShowCustomError;
}

- (void) freeNPTournamentPlayerRecursively:(_NPTournamentPlayer*)player
{
    if (player == NULL)
    {
        return;
    }
    
    if (player->name != NULL)
    {
        free(player->name);
        player->name = NULL;
    }
    
    if (player->playerId != NULL)
    {
        free(player->playerId);
        player->playerId = NULL;
    }
    
    if (player->imageUrl != NULL)
    {
        free(player->imageUrl);
        player->imageUrl = NULL;
    }
    
    free(player);
    player = NULL;
}

- (_NPTournamentPlayer*) createNPTournamentPlayerStruct:(NPTournamentPlayer*)player
{
    _NPTournamentPlayer* result = (_NPTournamentPlayer*)malloc(sizeof(_NPTournamentPlayer));
    
    [self fillNPTournamentPlayerStruct:result withPlayerInfo:player];
    
    return result;
}

- (void) fillNPTournamentPlayerStruct:(_NPTournamentPlayer*)playerStruct withPlayerInfo:(NPTournamentPlayer*)playerObject
{
    playerStruct->name = strdup([playerObject.playerName UTF8String]);
    playerStruct->playerId = strdup([playerObject.playerId UTF8String]);
    playerStruct->imageUrl = strdup([playerObject.imageUrl UTF8String]);
    playerStruct->isBot = playerObject.playerIsBot;
    playerStruct->isCurrentUser = playerObject.isCurrentUser;
}

- (void) nextpeerDidTournamentStartWithDetails:(NPTournamentStartDataContainer *)tournamentContainer
{
    [self freeNPTournamentPlayerRecursively:TournamentStart.mCurrentPlayer];
    TournamentStart.mCurrentPlayer = [self createNPTournamentPlayerStruct:tournamentContainer.currentPlayer];
    
    if (TournamentStart.mOpponents != NULL)
    {
        free(TournamentStart.mOpponents);
        TournamentStart.mOpponents = NULL;
    }
    TournamentStart.mOpponents = (_NPTournamentPlayer*)malloc(sizeof(_NPTournamentPlayer) * (tournamentContainer.numberOfPlayers-1));
    for (int opponentIndex = 0; opponentIndex < tournamentContainer.numberOfPlayers-1; opponentIndex++)
    {
        [self fillNPTournamentPlayerStruct:(TournamentStart.mOpponents + opponentIndex) withPlayerInfo:tournamentContainer.opponents[opponentIndex]];
    }
    
    const char* tournamentUuid = [[tournamentContainer tournamentUuid] UTF8String];
    if (TournamentStart.mTournamentUuid != NULL)
    {
        free(TournamentStart.mTournamentUuid);
        TournamentStart.mTournamentUuid = NULL;
    }
    TournamentStart.mTournamentUuid = new char[strlen(tournamentUuid) + 1];
    strcpy(TournamentStart.mTournamentUuid, tournamentUuid);
    
    const char* tournamentName = [[tournamentContainer tournamentName] UTF8String];
    if (TournamentStart.mTournamentName != NULL)
    {
        free(TournamentStart.mTournamentName);
        TournamentStart.mTournamentName = NULL;
    }
    TournamentStart.mTournamentName = new char[strlen(tournamentName) + 1];
    strcpy(TournamentStart.mTournamentName, tournamentName);

    TournamentStart.mTournamentRandomSeed = [tournamentContainer tournamentRandomSeed];
    TournamentStart.mNumberOfPlayers = [tournamentContainer numberOfPlayers];
    
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DID_TOURNAMENT_START_WITH_DETAILS,
                     "");
}

- (void) nextpeerDashboardWillAppear
{
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DASHBOARD_WILL_APPEAR,
                     "");
}

- (void) nextpeerDashboardDidAppear
{
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DASHBOARD_DID_APPEAR,
                     "");
}

- (void) nextpeerDashboardDidDisappear
{
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DASHBOARD_DID_DISAPPEAR,
                     "");
}

- (void) nextpeerDashboardWillDisappear
{
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DASHBOARD_WILL_DISAPPEAR,
                     "");
}

- (void) nextpeerDashboardDidReturnToGame
{
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DASHBOARD_DID_RETURN_TO_GAME,
                     "");
}

-(void) nextpeerDidTournamentEnd
{
    // Automatically release all the tournament custom messages if any
    [[NPTournamentObjectsContainer sharedInstance] clearContainer];
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     NP_DID_TOURNAMENT_END,
                     "");
}

@end
