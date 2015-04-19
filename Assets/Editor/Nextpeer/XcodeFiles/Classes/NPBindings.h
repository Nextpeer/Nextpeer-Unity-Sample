#import "Nextpeer/Nextpeer.h"

#ifndef Unity_iPhone_Header_h
#define Unity_iPhone_Header_h

/*
 Marshalling guide
 =================
 
 Discussion:
 In passing structs between C and C#, there are a number of points we need to be aware of:
 1) Size of the structure fields.
 2) Padding of the structure fiels.
 3) Handling strings.
 4) Handling other memory buffers (such as arrays), allocated either by the Objective C runtime or manually by us.
 
 Mostly problems can occur in arm64, where both the size and alignment of some data types changes. This can both mess up the size of each field, and the padding of the struct. One part of the answer to this is to make sure to use only fixed-size data types on the C side (C# doesn't have variable-size data types). The other part of the answer - struct padding - is that for now we assume that Mono's padding rules match the platform's.
 
 In the worst case scenario, it should be possible to transition to a hyper-paranoid system - use a constant packing for all structs, substitute bool for uint8_t (choose one or both, as needed).
 
 Be careful when passing enums. In C#, enums have an underlying type of 'int' (altough this can be changed), but this is not always true with iOS enums.
 
 Mono can marshal char* UTF-8 C strings into C# strings. It's also fine to use IntPtr on the C# side and call the marshaller manually. Performance-wise it should be the same, since the underlying buffer is always copied.
 
 In our code, we use both 'const char*' and 'char*' to pass strings to C#. The distinction is ad hoc - 'char*' fields are those which are allocated by our bridge code and must be released by us, so they can't get a 'const' qualifier. However, we still shouldn't modify those strings after allocating them, never know when C# may decide to start marshalling them.
 
 In general, when passing memory buffers (strings included), we should never transfer ownership to Mono, because it uses its own memory freeing functions, which are not guaranteed to match the functions used to create those buffers (since the buffers are created by the native code, not Mono). Mono can assume ownership of buffers, for exampe, when a buffer is a struct field, and the struct is passed by reference from C# to C, and its fields are populated in C, or when buffers are return values. To avoid this, you can use IntPtr on the C# side, and marshal manually as needed.
 
 Assumptions made:
 1) 'bool' is 1 byte in all architectures. (Apple's guide says so.)
 2) Mono's LayoutKind.Sequential (the default LayoutKind for structs) will match LLVM's struct padding.
 3) Mono always uses UTF-8 when marshalling strings (instead of Microsoft's ANSI). (Says so in the P/Invoke guide.)
 4) Returning -[NSString UTF8String] into C# is fine, because the C# processing happens within the context of a single run loop => the autorelease pool shouldn't be drained until the C# code finishes => the returned const char* should be valid until the C# code finishes.
 
 Best practices:
 1) Always use 'bool' to pass boolean values. The corresponding type in C# is Boolean, and it should have the attribute of [MarshalAs(UnmanagedType.I1)].
 2) Don't use variable-length data types in marshalling (mostly NS* or CG* types). While 'int' is safe (as it is 4 bytes on all iOS architectures), 'long' isn't. Consult Apple's guides to verify which types are 'safe', or use explicit typedefs ( [u]int(8|16|32|64)_t ).
 3) When memory buffers (e.g., strings), make sure no ownership tansfer is involved - that is, memory that is allocated by the native code should always be released by the native code.
 4) The order of struct fields must be the same in C# and C.
 5) When passing enums, make sure the underlying enum types match (or use an appropriate primitive data type to pass the enum value).
 
 References:
 The Mono P/Invoke guide: http://www.mono-project.com/docs/advanced/pinvoke/
 Apple's data type size chart: https://developer.apple.com/Library/ios/documentation/General/Conceptual/CocoaTouch64BitGuide/Major64-BitChanges/Major64-BitChanges.html#//apple_ref/doc/uid/TP40013501-CH2-SW8
 C struct padding across compilers: http://stackoverflow.com/questions/10298113/c-struct-alignment-and-portability-across-compilers
 */

// Structures returned to Unity
struct _NPGamePlayerContainer
{
    const char* PlayerId;
    const char* PlayerName;
    const char* ProfileImageURL;
};

struct _NPGameSettings
{
    int32_t notificationOrientation;
    bool observeNotificationOrientationChange;
    int32_t notificationPosition;
    bool supportsDasboardRotation;
    int32_t initialDashboardOrientation;
    int32_t rankingDisplayStyle;
    int32_t rankingDisplayAnimationStyle;
    int32_t rankingdisplayAlignment;
};

struct _NPTournamentPlayer
{
    char* name;
    char* playerId;
    char* imageUrl;
    bool isBot;
    bool isCurrentUser;
    bool isStillPlaying;
    bool didForfeit;
    uint32_t score;
};

struct _NPTournamentStartDataContainer
{
    char* mTournamentUuid;
    char* mTournamentName;
    uint32_t mTournamentRandomSeed;
    uint32_t mNumberOfPlayers;
    _NPTournamentPlayer* mCurrentPlayer;
    _NPTournamentPlayer* mOpponents;
};

struct _NPTournamentCustomMessageContainer
{
    const char* playerName;
    const char* profileImageURL;
    int32_t messageSize;
    const char* message;
    const char* playerId;
    bool playerIsBot;
};

struct _NPSyncEventInfo
{
    const char* syncEventName;
    int32_t syncEventFireReason;
};

struct _NPTournamentStatusInfo
{
    int32_t numberOfResults;
    _NPTournamentPlayer* results;
};

extern "C" {
    // TournamentIds list accessors
    void _NPAddWhitelistTournamentId(const char* tournamentId);
    void _NPRemoveWhitelistTournamentId(const char* tournamentId);
    void _NPClearTournamentWhitelist();
    
    void _NPAddBlacklistTournamentId(const char* tournamentId);
    void _NPRemoveBlacklistTournamentId(const char* tournamentId);
    void _NPClearTournamentBlacklist();
    
    // NextpeerNotSupportedShouldShowCustomError flag
    void _NPSetNextpeerNotSupportedShouldShowErrors(bool ShouldShow);
    bool _NPGetNextpeerNotSupportedShouldShowErrors();
    
    //Recording manipulation
    void _NPReportScoreModifier (const char* userId, int32_t scoreModifier);
    void _NPRequestFastForwardRecording (const char* userId, uint32_t timeDeltaMilliseconds);
    void _NPRequestPauseRecording (const char* userId);
    void _NPRequestResumeRecording (const char* userId);
    void _NPRequestRewindRecording (const char* userId, uint32_t timeDeltaMilliseconds);
	void _NPRequestStopRecording (const char* userId);
    
    // Obj-C Nextpeer
    // --------------
    // Nextpeer.h
    const char* _NPReleaseVersionString();
    
    // Init methods with delegates and settings
    void _NPInitSettings(const char *GameKey, _NPGameSettings* settings);
    
    // Nextpeer delegates accessors
    _NPTournamentStartDataContainer _NPGetTournamentStartData();
    void _NPLaunchDashboard();
    bool _NPIsNextpeerSupported();
    _NPGamePlayerContainer _NPGetCurrentPlayerDetails();
    void _NPPushDataToOtherPlayers(const char *data, int32_t size);
    void _NPUnreliablePushDataToOtherPlayers(const char *data, int32_t size);
    void _NPReportScoreForCurrentTournament(uint32_t score);
    void _NPRegisterToSynchronizedEvent(const char *data, double timeout); // timeout is in seconds (NSTimeInterval)
    bool _NPIsCurrentlyInTournament();
    void _NPReportForfeitForCurrentTournament();
    void _NPReportControlledTournamentOverWithScore(uint32_t score);
    
    // Tournament
    bool _NPConsumeCustomMessage(const char* MessageID, _NPTournamentCustomMessageContainer* message);
    _NPTournamentStatusInfo _NPConsumeTournamentStatusInfo(const char* MessageID);
    void _NPRemoveStoredObjectWithId(const char* messageID);
    bool _NPConsumeSyncEvent(const char* syncEventObjectId, _NPSyncEventInfo* syncEventInfo);
    
    // Notifications
    void _NPEnableRankingDisplay(bool enableRankingDisplay);
}
#endif