using UnityEngine;
using System.Collections;
using System;

#if UNITY_IPHONE || UNITY_ANDROID

public interface INextpeer
{
	bool IsNextpeerSupported();
	string ReleaseVersionString();
	void Init(string GameKey, NPGameSettings? Settings=null);
	void LaunchDashboard();
	void DismissDashboard();
	bool IsCurrentlyInTournament();
	void ReportScoreForCurrentTournament(UInt32 score);
	void ReportControlledTournamentOverWithScore(UInt32 score);
	void ReportForfeitForCurrentTournament();
	TimeSpan TimeLeftInTournament();
	void PushDataToOtherPlayers(byte[] data);
	void UnreliablePushDataToOtherPlayers(byte[] data);
	void AddWhitelistTournament(string tournamentId);
	void RemoveWhitelistTournament(string tournamentId);
	void ClearTournamentWhitelist();
	void AddBlacklistTournament(string tournamentId);
	void RemoveBlacklistTournament(string tournamentId);
	void ClearTournamentBlacklist();
	void PostToFacebookWall(String message, String link, String ImageUrl);
	NPGamePlayerContainer GetCurrentPlayerDetails();
	void OpenFeedDashboard();
	Int32 GetCurrencyAmount();
	void SetCurrencyAmount(Int32 amount);
	void SetSupportsUnifiedCurrency(Boolean supported);
	//void SetNextpeerNotificationAllowed(Boolean isAllowed);
	void EnableRankingDisplay(bool enableRankingDisplay);
	void SetAllowInterGameScreen(Boolean allowInterGameScreen);
	void ResumePlayAgainLogic();
	void SetNextpeerNotSupportedShouldShowCustomErrors(Boolean ShowError);
	void RegisterToSyncEvent(string eventName, TimeSpan timeout);
	
	// Non-SDK API:
	NPTournamentStartDataContainer GetTournamentStartData();
	NPTournamentCustomMessageContainer ConsumeReliableCustomMessage(String id);
	void RemoveStoredObjectWithId(String MessageID);
	NPTournamentUnreliableCustomMessageContainer ConsumeUnreliableCustomMessage(String id);
	NPTournamentStatusInfo? ConsumeTournamentStatusInfo(String MessageID);
	NPTournamentEndDataContainer GetTournamentResult();
	bool ConsumeSyncEventInfo(string syncEventInfoId, ref string eventName, ref NPSynchronizedEventFireReason fireReason);
}

#endif
