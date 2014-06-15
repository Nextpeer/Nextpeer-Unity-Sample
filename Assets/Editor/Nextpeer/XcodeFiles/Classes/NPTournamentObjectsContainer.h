#import <Foundation/Foundation.h>

@interface NPTournamentObjectsContainer : NSObject

+(id)sharedInstance;

-(void)storeObject:(id)object andSendMessageToUnityMethod:(const char*)unityMethod;
-(id)retrieveObjectForId:(const char*)objectId;
-(void)removeObjectForId:(const char*)objectId;
-(void)clearContainer;

@end
