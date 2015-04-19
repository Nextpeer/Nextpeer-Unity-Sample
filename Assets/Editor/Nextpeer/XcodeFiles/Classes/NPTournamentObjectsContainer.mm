#import "NPTournamentObjectsContainer.h"
#import "NPConstants.h"

extern void UnitySendMessage(const char *, const char *, const char *);

@interface NPTournamentObjectsContainer ()
{
    NSMutableDictionary* mObjectsContainer;
}

@end

@implementation NPTournamentObjectsContainer

static NPTournamentObjectsContainer* sharedInstance;
+(id)sharedInstance
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [self new];
    });
    
    return sharedInstance;
}

-(id)init
{
    if (!(self = [super init]))
    {
        return nil;
    }
    
    mObjectsContainer = [NSMutableDictionary new];
    
    return self;
}

-(void)storeObject:(id)object andSendMessageToUnityMethod:(const char*)unityMethod
{
    CFUUIDRef theUUID = CFUUIDCreate(NULL);
    NSString* Key = (__bridge_transfer NSString*)CFUUIDCreateString(NULL, theUUID);
    CFRelease(theUUID);
    
    mObjectsContainer[Key] = object;
    
    UnitySendMessage(NP_GAMEOBJECTPATH,
                     unityMethod,
                     [Key cStringUsingEncoding:NSASCIIStringEncoding]);
}

-(void)removeObjectForId:(const char *)objectId
{
    [mObjectsContainer removeObjectForKey:[NSString stringWithUTF8String:objectId]];
}

-(void)clearContainer
{
    [mObjectsContainer removeAllObjects];
}

-(id)retrieveObjectForId:(const char *)objectId
{
    return mObjectsContainer[[NSString stringWithUTF8String:objectId]];
}

@end
