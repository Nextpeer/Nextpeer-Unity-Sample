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
	bool IsCurrentlyInTournament();
	void ReportScoreForCurrentTournament(UInt32 score);
	void ReportControlledTournamentOverWithScore(UInt32 score);
	void ReportForfeitForCurrentTournament();
	void PushDataToOtherPlayers(byte[] data);
	void UnreliablePushDataToOtherPlayers(byte[] data);
	void AddWhitelistTournament(string tournamentId);
	void RemoveWhitelistTournament(string tournamentId);
	void ClearTournamentWhitelist();
	void AddBlacklistTournament(string tournamentId);
	void RemoveBlacklistTournament(string tournamentId);
	void ClearTournamentBlacklist();
	NPGamePlayerContainer GetCurrentPlayerDetails();
	void EnableRankingDisplay(bool enableRankingDisplay);
	void SetNextpeerNotSupportedShouldShowCustomErrors(Boolean ShowError);
	void RegisterToSyncEvent(string eventName, TimeSpan timeout);
	void CaptureMoment();
	//Recording manipulation
	void ReportScoreModifier (String userId, Int32 scoreModifier);
	void RequestFastForwardRecording (String userId, TimeSpan timeDelta);
	void RequestPauseRecording (String userId);
	void RequestResumeRecording (String userId);
	void RequestRewindRecording(String userId, TimeSpan timeDelta);
	void RequestStopRecording(String userId);


	
	// Non-SDK API:
	NPTournamentStartDataContainer GetTournamentStartData();
	NPTournamentCustomMessageContainer ConsumeReliableCustomMessage(String id);
	void RemoveStoredObjectWithId(String MessageID);
	NPTournamentUnreliableCustomMessageContainer ConsumeUnreliableCustomMessage(String id);
	NPTournamentStatusInfo? ConsumeTournamentStatusInfo(String MessageID);
	bool ConsumeSyncEventInfo(string syncEventInfoId, ref string eventName, ref NPSynchronizedEventFireReason fireReason);
	
	//Android only
	#if UNITY_ANDROID
	NPTournamentEndDataContainer GetTournamentResult();
	bool FrameStart(int width, int height);
    void FrameEnd();
	#endif

}

#endif
