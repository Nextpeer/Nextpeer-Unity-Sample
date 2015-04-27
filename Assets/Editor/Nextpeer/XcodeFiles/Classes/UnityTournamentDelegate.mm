#include "UnityTournamentDelegate.h"
#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

#include "NPConstants.h"
#import "NPTournamentObjectsContainer.h"

#import <Nextpeer/Nextpeer.h>

extern void UnitySendMessage(const char *, const char *, const char *);



@interface NPSyncEventInfo : NSObject

@property (nonatomic, strong) NSString* eventName;
@property (nonatomic, assign) NPSynchronizedEventFireReason fireReason;

@end

@implementation NPSyncEventInfo

@end


@implementation UnityTournamentDelegate

// Tournament custom message data access
//------------------------------------------------

-(BOOL)consumeCustomMessageWithId:(const char*)MessageID message:(_NPTournamentCustomMessageContainer*)message;
{
    // IMPORTANT: this code has a very unsafe optimization. To prevent extra array copying, we rely on the fact that in our C# code we immediately marshal the pointers that were populated by this method.
    // So, autoreleasing these buffers is OK from our Objective C perspective, since by the next runloop iteration, all the data will have been marshalled.
    // TODO: actually, this optimization can be obviated by using -[NSString dataUsingEncoding]. This will require us to add a "clear message with ID" function, but a small price to pay for safety?
    // TODO: as an optimization, we can intern all the repeatable strings (profileName, profileImageUrl, playerId).
    
    NPTournamentCustomMessageContainer* npMessage = [[NPTournamentObjectsContainer sharedInstance] retrieveObjectForId:MessageID];
    
    if (npMessage == nil)
    {
        WARN_DEVELOPER(@"Couldn't retrieve message for key %s, no such message exists.", MessageID);
        return NO;
    }
    
    // Marshal.PtrToStringAnsi uses UTF-8 on iOS, so we use that.
    message->playerName = [npMessage.playerName UTF8String];
    message->profileImageURL = [npMessage.playerImageUrl UTF8String];
    message->messageSize = [npMessage.message length];
    message->message = (const char*)[npMessage.message bytes];
    message->playerId = [npMessage.playerId UTF8String];
    message->playerIsBot = [npMessage playerIsBot];
    
    char* messageIdCopy = strdup(MessageID);
    dispatch_async(dispatch_get_main_queue(), ^() {
        [[NPTournamentObjectsContainer sharedInstance] removeObjectForId:messageIdCopy];
        free(messageIdCopy);
    });
    
    return YES;
}

-(BOOL)consumeSyncEventWithId:(const char *)infoObjectId infoObject:(_NPSyncEventInfo *)syncEventInfoStruct
{
    NPSyncEventInfo* syncEventInfo = [[NPTournamentObjectsContainer sharedInstance] retrieveObjectForId:infoObjectId];
    
    if (syncEventInfo == nil)
    {
        WARN_DEVELOPER(@"Couldn't retrieve sync event for key %s, no such sync event exists.", infoObjectId);
        return NO;
    }
    
    syncEventInfoStruct->syncEventName = [syncEventInfo.eventName UTF8String];
    syncEventInfoStruct->syncEventFireReason = syncEventInfo.fireReason;
    
    char* infoObjectIdCopy = strdup(infoObjectId);
    dispatch_async(dispatch_get_main_queue(), ^() {
        [[NPTournamentObjectsContainer sharedInstance] removeObjectForId:infoObjectIdCopy];
        free(infoObjectIdCopy);
    });
    
    return YES;
}

// Tournament End data access
//------------------------------------------------

- (void) nextpeerDidReceiveTournamentCustomMessage:(NPTournamentCustomMessageContainer*)message
{
    [[NPTournamentObjectsContainer sharedInstance] storeObject:message andSendMessageToUnityMethod:NP_DID_RECEIVE_TOURNAMENT_CUSTOM_MESSAGE];
}

-(void)nextpeerDidReceiveUnreliableTournamentCustomMessage:(NPTournamentCustomUnreliableMessageContainer *)message
{
    [[NPTournamentObjectsContainer sharedInstance] storeObject:message andSendMessageToUnityMethod:NP_DID_RECEIVE_UNRELIABLE_TOURNAMENT_CUSTOM_MESSAGE];
}

-(void)nextpeerDidReceiveSynchronizedEvent:(NSString *)eventName withReason:(NPSynchronizedEventFireReason)fireReason
{
    NPSyncEventInfo* syncEventInfo = [NPSyncEventInfo new];
    syncEventInfo.eventName = eventName;
    syncEventInfo.fireReason = fireReason;
    
    [[NPTournamentObjectsContainer sharedInstance] storeObject:syncEventInfo andSendMessageToUnityMethod:NP_DID_RECEIVE_SYNC_EVENT];
}

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
        size_t sizeOfResultsArray = result.numberOfResults * sizeof(_NPTournamentPlayer);
        result.results = (_NPTournamentPlayer*)malloc(sizeOfResultsArray);
        
        for (int resultIndex = 0; resultIndex < result.numberOfResults; resultIndex++)
        {
            NPTournamentPlayer* npCurrentPlayer = statusInfo.sortedResults[resultIndex];
            _NPTournamentPlayer* currentPlayerResult = result.results + resultIndex;
            
            
            currentPlayerResult->name = strdup([npCurrentPlayer.playerName UTF8String]);
            currentPlayerResult->playerId = strdup([npCurrentPlayer.playerId UTF8String]);
            currentPlayerResult->imageUrl = strdup([npCurrentPlayer.imageUrl UTF8String]);
            currentPlayerResult->isBot = npCurrentPlayer.playerIsBot;
            currentPlayerResult->isCurrentUser = [npCurrentPlayer isCurrentUser];
            currentPlayerResult->isStillPlaying = npCurrentPlayer.isStillPlaying;
            currentPlayerResult->didForfeit = npCurrentPlayer.didForfeit;
            currentPlayerResult->score = [[npCurrentPlayer score] unsignedIntValue];
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
