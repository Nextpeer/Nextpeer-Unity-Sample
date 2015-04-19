#ifndef Unity_iPhone_NPConstants_h
#define Unity_iPhone_NPConstants_h

// Default values
#define NP_GAMEOBJECTPATH                                   "Nextpeer"

// Methods strings
#define NP_DID_TOURNAMENT_START_WITH_DETAILS                "DidTournamentStartWithDetailsHandler"
#define NP_DID_TOURNAMENT_END                               "DidTournamentEndHandler"
#define NP_DASHBOARD_WILL_APPEAR                            "DashboardWillAppearHandler"
#define NP_DASHBOARD_DID_APPEAR                             "DashboardDidAppearHandler"
#define NP_DASHBOARD_WILL_DISAPPEAR                         "DashboardWillDisappearHandler"
#define NP_DASHBOARD_DID_DISAPPEAR                          "DashboardDidDisappearHandler"
#define NP_DASHBOARD_DID_RETURN_TO_GAME                     "DashboardDidReturnToGameHandler"
#define NP_DID_RECEIVE_TOURNAMENT_CUSTOM_MESSAGE            "DidReceiveTournamentCustomMessageHandler"
#define NP_DID_RECEIVE_UNRELIABLE_TOURNAMENT_CUSTOM_MESSAGE "DidReceiveUnreliableTournamentCustomMessageHandler"
#define NP_DID_RECEIVE_TOURNAMENT_STATUS_INFO               "DidReceiveTournamentStatusInfoHandler"
#define NP_DID_RECEIVE_SYNC_EVENT                           "DidReceiveSynchronizedEventHandler"

#define WARN_DEVELOPER(warning, ...) do { NSLog(@"\n==============================\n\nNextpeer warning: %@\n\n==============================", [NSString stringWithFormat:warning, ##__VA_ARGS__]); } while (0)

#endif
