#include "UnityNotificationDelegate.h"

#include "NPConstants.h"
#include "iPhone_target_Prefix.pch"
#import "NextpeerAppController.h"
#import "NPTournamentObjectsContainer.h"

@implementation UnityNotificationDelegate

-(void)nextpeerDidReceiveTournamentStatus:(NPTournamentStatusInfo *)tournamentStatus
{
    [[NPTournamentObjectsContainer sharedInstance] storeObject:tournamentStatus andSendMessageToUnityMethod:NP_DID_RECEIVE_TOURNAMENT_STATUS_INFO];
}

-(_NPTournamentStatusInfo)consumeTournamentStatusInfoWithId:(const char *)statusId
{
    // IMPORTANT: this code has a very unsafe optimization. To prevent extra array copying, we rely on the fact that in our C# code we immediately marshal the pointers that were populated by this method.
    // So, autoreleasing these buffers is OK from our Objective C perspective, since by the next runloop iteration, all the data will have been marshalled.
    
    _NPTournamentStatusInfo result = {};
    
    NPTournamentStatusInfo* statusInfo = [[NPTournamentObjectsContainer sharedInstance] retrieveObjectForId:statusId];
    if (statusInfo != nil)
    {
        result.numberOfResults = (int)statusInfo.sortedResults.count;
        size_t sizeOfResultsArray = result.numberOfResults * sizeof(_NPTournamentPlayerResults);
        result.results = (_NPTournamentPlayerResults*)malloc(sizeOfResultsArray);
        
        for (int resultIndex = 0; resultIndex < result.numberOfResults; resultIndex++)
        {
            NPTournamentPlayerResults* npCurrentPlayerResult = statusInfo.sortedResults[resultIndex];
            NPTournamentPlayer* npCurrentPlayer = npCurrentPlayerResult.player;
            _NPTournamentPlayerResults* currentPlayerResult = result.results + resultIndex;
            
            currentPlayerResult->playerName = [npCurrentPlayer.playerName UTF8String];
            currentPlayerResult->playerId = [npCurrentPlayer.playerId UTF8String];
            currentPlayerResult->playerImageUrl = [npCurrentPlayer.playerImageUrl UTF8String];
            currentPlayerResult->playerIsBot = npCurrentPlayer.playerIsBot;
            currentPlayerResult->playerIsCurrentUser = [npCurrentPlayer isCurrentUser];
            currentPlayerResult->isStillPlaying = npCurrentPlayerResult.isStillPlaying;
            currentPlayerResult->didForfeit = npCurrentPlayerResult.didForfeit;
            currentPlayerResult->score = npCurrentPlayerResult.score;
        }
        
        // This is OK because of our optimization assumption (see above).
        char* statusIdCopy = strdup(statusId);
        dispatch_async(dispatch_get_main_queue(), ^() {
            free(result.results);
            [[NPTournamentObjectsContainer sharedInstance] removeObjectForId:statusIdCopy];
            free(statusIdCopy);
        });
    }
    
    return result;
}

@end