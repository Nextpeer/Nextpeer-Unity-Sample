#import "Nextpeer/Nextpeer.h"

#ifndef Unity_iPhone_Header_h
#define Unity_iPhone_Header_h

// Structures returned to Unity
struct _NPGamePlayerContainer
{
    const char* PlayerId;
    const char* PlayerName;
    const char* ProfileImageURL;
    bool IsSocialAuthenticated;
};

struct _NPGameSettings
{
    const char* displayName;
    int notificationOrientation;
    bool observeNotificationOrientationChange;
    int notificationPosition;
    bool supportsDasboardRotation;
    int initialDashboardOrientation;
    int rankingDisplayStyle;
    int rankingDisplayAnimationStyle;
    int rankingdisplayAlignment;
};

struct _NPTournamentPlayer
{
    char* name;
    char* playerId;
    char* imageUrl;
    bool isBot;
    bool isCurrentUser;
};

struct _NPTournamentStartDataContainer
{
    char* mTournamentUuid;
    char* mTournamentName;
    unsigned int mTournamentTimeSeconds;
    unsigned int mTournamentRandomSeed;
    bool mTournamentIsGameControlled;
    unsigned int mNumberOfPlayers;
    _NPTournamentPlayer* mCurrentPlayer;
    _NPTournamentPlayer* mOpponents;
};

struct _NPTournamentEndDataContainer
{
    char* mTournamentUuid;
    char* mPlayerName;
    int mTournamentTotalPlayers;
    int mCurrentCurrencyAmount;
    int mPlayerRankInTournament;
    int mPlayerScore;
};

struct _NPTournamentCustomMessageContainer
{
    const char* playerName;
    const char* profileImageURL;
    int messageSize;
    const char* message;
    const char* playerId;
    bool playerIsBot;
};

struct _NPSyncEventInfo
{
    const char* syncEventName;
    int syncEventFireReason;
};

struct _NPTournamentPlayerResults
{
    const char* playerName;
    const char* playerId;
    const char* playerImageUrl;
    bool playerIsBot;
    bool playerIsCurrentUser;
    bool isStillPlaying;
    bool didForfeit;
    uint32_t score;
};

struct _NPTournamentStatusInfo
{
    int numberOfResults;
    _NPTournamentPlayerResults* results;
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
    
    // Obj-C Nextpeer
    // --------------
    // Nextpeer.h
    const char* _NPReleaseVersionString();
    
    // Init methods with delegates and settings
    void _NPInitSettings(const char *GameKey, _NPGameSettings* settings);
    
    // Nextpeer delegates accessors
    _NPTournamentStartDataContainer _NPGetTournamentStartData();
    void _NPLaunchDashboard();
    void _NPDismissDashboard();
    bool _NPIsNextpeerSupported();
    void _NPPostToFacebookWallMessage(const char *message, const char *link,const char *imageUrl);
    _NPGamePlayerContainer _NPGetCurrentPlayerDetails();
    void _NPPushDataToOtherPlayers(const char *data, int size);
    void _NPUnreliablePushDataToOtherPlayers(const char *data, int size);
    void _NPReportScoreForCurrentTournament(uint32_t score);
    void _NPRegisterToSynchronizedEvent(const char *data, double timeout); // timeout is in seconds (NSTimeInterval)
    bool _NPIsCurrentlyInTournament();
    void _NPReportForfeitForCurrentTournament();
    void _NPReportControlledTournamentOverWithScore(uint32_t score);
    NSUInteger _NPTimeLeftInTournament();
    
    // Tournament
    bool _NPConsumeCustomMessage(const char* MessageID, _NPTournamentCustomMessageContainer* message);
    _NPTournamentStatusInfo _NPConsumeTournamentStatusInfo(const char* MessageID);
    void _NPRemoveStoredObjectWithId(const char* messageID);
    _NPTournamentEndDataContainer _NPGetTournamentResult();
    bool _NPConsumeSyncEvent(const char* syncEventObjectId, _NPSyncEventInfo* syncEventInfo);
    
    // Inter-game screen logic
    void _NPSetShouldAllowInterGameScreen(bool allowInterGameScreen);
    void _NPResumePlayAgainLogic();
    
    // Currency
    NSInteger _NPGetCurrencyAmount();
    void _NPSetCurrencyAmount(NSInteger amount);
    bool _NPIsUnifiedCurrencySupported();
    void _NPSwitchUnifiedCurrencySupported(bool isSupported);
    
    // Feeds
    void _NPOpenFeedDashboard();
    
    // Notifications
    void _NPEnableRankingDisplay(bool enableRankingDisplay);
}
#endif