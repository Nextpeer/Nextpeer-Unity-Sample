using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

#if UNITY_IPHONE

public class NextpeerIOS : INextpeer
{
	#region Interface implementation
	
	public bool IsNextpeerSupported()
    {
        return _NPIsNextpeerSupported();
	}
	
	public string ReleaseVersionString()
    {
        return Marshal.PtrToStringAnsi(_NPReleaseVersionString());
    }
	
	public void Init(String GameKey, NPGameSettings? Settings=null)
    {
		// Convenience variable, to save us all the "Value" calls.
		NPGameSettings userSettings = Settings ?? new NPGameSettings();
		
		MSettings settingsVal = new MSettings();
		
		// Setting defaults for unspecified values, to avoid dealing with passing nulls to Objective-C++:
		settingsVal.NotificationOrientation = userSettings.NotificationOrientation ?? 0;
		settingsVal.ObserveNotificationOrientationChange = userSettings.ObserveNotificationOrientationChange ?? false;
		settingsVal.NotificationPosition = userSettings.NotificationPosition ?? NPNotificationPosition.NPNotificationPosition_TOP;
		settingsVal.SupportsDashboardRotation = userSettings.SupportsDashboardRotation ?? false;
		settingsVal.RankingDisplayStyle = userSettings.RankingDisplayStyle ?? 0;
		settingsVal.RankingDisplayAnimationStyle = userSettings.RankingDisplayAnimationStyle ?? 0;
		settingsVal.RankingDisplayAlignment = userSettings.RankingDisplayAlignment ?? 0;
		// The default is determined at runtime, so by using 0 we indicate a "no value set" case:
		settingsVal.InitialDashboardOrientation = userSettings.InitialDashboardOrientation ?? 0;

        _NPInitSettings(GameKey, ref settingsVal);
    }
	
	public void LaunchDashboard()
    {
        _NPLaunchDashboard();
    }
	
	public bool IsCurrentlyInTournament()
    {
        return _NPIsCurrentlyInTournament();
    }
	
	public void ReportScoreForCurrentTournament(UInt32 score)
    {
        _NPReportScoreForCurrentTournament(score);
    }
	
	public void ReportControlledTournamentOverWithScore(UInt32 score)
    {
        _NPReportControlledTournamentOverWithScore(score);
    }
	
	public void ReportForfeitForCurrentTournament()
    {
        _NPReportForfeitForCurrentTournament();
    }
	
	public void PushDataToOtherPlayers(byte[] data)
    {
        _NPPushDataToOtherPlayers(data, data.Length);
    }
	
	public void UnreliablePushDataToOtherPlayers(byte[] data)
    {
        _NPUnreliablePushDataToOtherPlayers(data, data.Length);
    }
	
	public void AddWhitelistTournament(string tournamentId)
	{
		_NPAddWhitelistTournamentId(tournamentId);
	}
	
	public void RemoveWhitelistTournament(string tournamentId)
	{
		_NPRemoveWhitelistTournamentId(tournamentId);
	}
	
	public void ClearTournamentWhitelist()
	{
		_NPClearTournamentWhitelist();
	}
	
	public void AddBlacklistTournament(string tournamentId)
	{
		_NPAddBlacklistTournamentId(tournamentId);
	}
	
	public void RemoveBlacklistTournament(string tournamentId)
	{
		_NPRemoveWhitelistTournamentId(tournamentId);
	}
	
	public void ClearTournamentBlacklist()
	{
		_NPClearTournamentWhitelist();
	}
	
	public NPGamePlayerContainer GetCurrentPlayerDetails()
    {
        return _NPGetCurrentPlayerDetails();
    }
	
	public void EnableRankingDisplay(bool enableRankingDisplay)
	{
		_NPEnableRankingDisplay(enableRankingDisplay);
	}
	
	
	public void SetNextpeerNotSupportedShouldShowCustomErrors(Boolean ShowError)
    {
        _NPSetNextpeerNotSupportedShouldShowErrors(ShowError);
    }
	
	public NPTournamentStartDataContainer GetTournamentStartData()
    {
		NPTournamentStartDataContainer result = new NPTournamentStartDataContainer();
		
        _NPTournamentStartDataContainer startData = _NPGetTournamentStartData();
		result.TournamentUUID = Marshal.PtrToStringAnsi(startData.TournamentUuid);
		result.TournamentName = Marshal.PtrToStringAnsi(startData.TournamentName);
		result.TournamentRandomSeed = startData.TournamentRandomSeed;
		result.TournamentNumberOfPlayers = startData.NumberOfPlayers;
		
		int tournamentPlayerStructSize = Marshal.SizeOf(typeof(_NPTournamentPlayer));
		result.CurrentPlayer = ConvertTournamentPlayer((_NPTournamentPlayer)Marshal.PtrToStructure(startData.CurrentPlayerPtr, typeof(_NPTournamentPlayer)));
		
		result.Opponents = new NPTournamentPlayer[startData.NumberOfPlayers-1];
		Int64 opponentsArrayHead = startData.OpponentsPtr.ToInt64();
		for (int opponentIndex = 0; opponentIndex < startData.NumberOfPlayers-1; opponentIndex++)
		{
			IntPtr opponentStructPtr = new IntPtr(opponentsArrayHead + opponentIndex*tournamentPlayerStructSize);
			result.Opponents[opponentIndex] = ConvertTournamentPlayer((_NPTournamentPlayer)Marshal.PtrToStructure(opponentStructPtr, typeof(_NPTournamentPlayer)));
		}
		
		return result;
    }
	
	private NPTournamentPlayer ConvertTournamentPlayer(_NPTournamentPlayer player)
	{
		NPTournamentPlayer result = new NPTournamentPlayer();
		
		result.PlayerName = Marshal.PtrToStringAnsi(player.Name);
		result.PlayerId = Marshal.PtrToStringAnsi(player.Id);
		result.PlayerImageUrl = Marshal.PtrToStringAnsi(player.ImageUrl);
		result.PlayerIsBot = player.IsBot;
		result.IsCurrentUser = player.IsCurrentUser;
		
		return result;
	}
	
	public NPTournamentCustomMessageContainer ConsumeReliableCustomMessage(String id)
	{
		return ConsumeCustomMessage(id);
	}
	
	public NPTournamentUnreliableCustomMessageContainer ConsumeUnreliableCustomMessage(String id)
	{
		NPTournamentCustomMessageContainer message = ConsumeCustomMessage(id);
		if (message == null)
		{
			return null;
		}
		
		NPTournamentUnreliableCustomMessageContainer result = new NPTournamentUnreliableCustomMessageContainer();
		
		result.PlayerName = message.PlayerName;
		result.PlayerID = message.PlayerID;
		result.ProfileImageUrl = message.ProfileImageUrl;
		result.PlayerIsBot = message.PlayerIsBot;
		result.Message = message.Message;
		
		return result;
	}
	
	public void RemoveStoredObjectWithId(string MessageId)
	{
		_NPRemoveStoredObjectWithId(MessageId);
	}
	
	public NPTournamentStatusInfo? ConsumeTournamentStatusInfo(String MessageID)
	{
		_NPTournamentStatusInfo marshalledStatusInfo = _NPConsumeTournamentStatusInfo(MessageID);
		if (marshalledStatusInfo.NumberOfResults == 0)
		{
			return null;
		}
		
		NPTournamentStatusInfo result;
		
		int tournamentPlayerResultsStructSize = Marshal.SizeOf(typeof(_NPTournamentPlayer));
		result.SortedResults = new NPTournamentPlayerResults[marshalledStatusInfo.NumberOfResults];
		for (int resultIndex = 0; resultIndex < marshalledStatusInfo.NumberOfResults; resultIndex++)
		{
			_NPTournamentPlayer marshalledPlayer = 
				(_NPTournamentPlayer)Marshal.PtrToStructure(new IntPtr(marshalledStatusInfo.SortedResultsPtr.ToInt64() + tournamentPlayerResultsStructSize*resultIndex),
					typeof(_NPTournamentPlayer));

			
			NPTournamentPlayer player = new NPTournamentPlayer() {
				PlayerName = Marshal.PtrToStringAnsi(marshalledPlayer.Name),
				PlayerId = Marshal.PtrToStringAnsi(marshalledPlayer.Id),
				PlayerImageUrl = Marshal.PtrToStringAnsi(marshalledPlayer.ImageUrl),
				PlayerIsBot = marshalledPlayer.IsBot,
				IsCurrentUser = marshalledPlayer.IsCurrentUser
			};
			
			NPTournamentPlayerResults playerResults = new NPTournamentPlayerResults() {
				Player = player,
				IsStillPlaying = marshalledPlayer.IsStillPlaying,
				DidForfeit = marshalledPlayer.DidForfeit,
				Score = marshalledPlayer.Score
			};
			
			result.SortedResults[resultIndex] = playerResults;
		}
		
		return result;
	}
	
	public void RegisterToSyncEvent(string eventName, TimeSpan timeout)
	{
		_NPRegisterToSynchronizedEvent(eventName, timeout.TotalSeconds);
	}

	public void CaptureMoment()
	{
		_NPCaptureMoment();
	}
	
	public bool ConsumeSyncEventInfo(string syncEventInfoId, ref string eventName, ref NPSynchronizedEventFireReason fireReason)
	{
		_NPSyncEventInfo syncEventInfo;
		if (!_NPConsumeSyncEvent(syncEventInfoId, out syncEventInfo))
		{
			return false;
		}
		
		eventName = Marshal.PtrToStringAnsi(syncEventInfo.EventName);
		fireReason = (NPSynchronizedEventFireReason)syncEventInfo.FireReason;
		
		return true;
	}

	//Recording manipulation
	public void ReportScoreModifier (String userId, Int32 scoreModifier){
		_NPReportScoreModifier(userId, scoreModifier);
	}
	public void RequestFastForwardRecording (String userId, TimeSpan timeDelta){
		_NPRequestFastForwardRecording(userId, (UInt32)timeDelta.TotalMilliseconds);
	}
	public void RequestPauseRecording (String userId){
		_NPRequestPauseRecording(userId);
	}
	public void RequestResumeRecording (String userId){
		_NPRequestResumeRecording(userId);
	}
	public void RequestRewindRecording(String userId, TimeSpan timeDelta){
		_NPRequestRewindRecording(userId, (UInt32)timeDelta.TotalMilliseconds);
	}
	public void RequestStopRecording(String userId){
		_NPRequestStopRecording(userId);
	}
	#endregion
	
	#region Bridge to Objective C
	
	// The marshalling guide for our bridge code is in NPBindings.h.
	
	// Marhsalling nullable types (used in NPGameSettings) isn't an option, so we need an intermediate struct to pass the settings to Objective C++.
	private struct MSettings
    {
        public NPUIInterfaceOrientation NotificationOrientation;
		[MarshalAs(UnmanagedType.I1)]
        public Boolean ObserveNotificationOrientationChange;
        public NPNotificationPosition NotificationPosition;
		[MarshalAs(UnmanagedType.I1)]
        public Boolean SupportsDashboardRotation;
		public NPUIInterfaceOrientation InitialDashboardOrientation;
		public NPRankingDisplayStyle RankingDisplayStyle;
		public NPRankingDisplayAnimationStyle RankingDisplayAnimationStyle;
		public NPRankingDisplayAlignment RankingDisplayAlignment;
    }
	
	private struct _NPTournamentCustomMessage
	{
		public IntPtr PlayerName;
		public IntPtr ProfilleImageUrl;
		public Int32 MessageSize;
		public IntPtr Message;
		public IntPtr PlayerID;
		public Boolean PlayerIsBot;
	}
	
	private struct _NPTournamentStatusInfo
	{
		public Int32 NumberOfResults;
		public IntPtr SortedResultsPtr;
	}
	
	private struct _NPSyncEventInfo
	{
		public IntPtr EventName;
		public Int32 FireReason;
	}
	
	private struct _NPTournamentPlayer
	{
		public IntPtr Name;
		public IntPtr Id;
		public IntPtr ImageUrl;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean IsBot;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean IsCurrentUser;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean IsStillPlaying;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean DidForfeit;
		public UInt32 Score;
	}
	
	private struct _NPTournamentStartDataContainer
	{
		public IntPtr TournamentUuid;
		public IntPtr TournamentName;
		public UInt32 TournamentRandomSeed;
		[MarshalAs(UnmanagedType.I1)]
		public UInt32 NumberOfPlayers;
		public IntPtr CurrentPlayerPtr;
		public IntPtr OpponentsPtr;
	}
	
	#region PInvoke calls

	[DllImport("__Internal")]
    private static extern IntPtr _NPReleaseVersionString();
	
	// Init:
	[DllImport("__Internal")]
    private static extern void _NPInitSettings(String Key, ref MSettings settings);

    // other methods  --------------------------------------------------------------------------------------------------------------------
    [DllImport("__Internal")]
    private static extern void _NPLaunchDashboard();
    [DllImport("__Internal")]
    private static extern bool _NPIsNextpeerSupported();
    [DllImport("__Internal")]
    private static extern NPGamePlayerContainer _NPGetCurrentPlayerDetails();
    [DllImport("__Internal")]
    private static extern void _NPPushDataToOtherPlayers(byte[] data, Int32 size);
	[DllImport("__Internal")]
    private static extern void _NPUnreliablePushDataToOtherPlayers(byte[] data, Int32 size);
    [DllImport("__Internal")]
    private static extern void _NPReportScoreForCurrentTournament(UInt32 score);
    [DllImport("__Internal")]
    private static extern bool _NPIsCurrentlyInTournament();
    [DllImport("__Internal")]
    private static extern void _NPReportForfeitForCurrentTournament();
	[DllImport("__Internal")]
    private static extern void _NPReportControlledTournamentOverWithScore(UInt32 score);
	[DllImport("__Internal")]
	private static extern void _NPRegisterToSynchronizedEvent(String eventName, double timeout);
	[DllImport("__Internal")]
	private static extern void _NPCaptureMoment();
	
	  // In memory data access dedicated methods
     // Tournament --------------------------------------------------------
    [DllImport("__Internal")]
    private static extern _NPTournamentStartDataContainer _NPGetTournamentStartData();		


    // Tournament IDs management methods -----------------------------------------------------------
    [DllImport("__Internal")]
    private static extern void _NPAddWhitelistTournamentId(String Id);
	[DllImport("__Internal")]
	private static extern void _NPRemoveWhitelistTournamentId(String Id);
	[DllImport("__Internal")]
	private static extern void _NPClearTournamentWhitelist();
    [DllImport("__Internal")]
    private static extern void _NPAddBlacklistTournamentId(String Id);
	[DllImport("__Internal")]
	private static extern void _NPRemoveBlacklistTournamentId(String Id);
	[DllImport("__Internal")]
	private static extern void _NPClearTournamentBlacklist();

	//Recording manipulation
	[DllImport("__Internal")]
	private static extern void _NPReportScoreModifier (String userId, Int32 scoreModifier);
	[DllImport("__Internal")]
	private static extern void _NPRequestFastForwardRecording (String userId, UInt32 timeDeltaMilliseconds);
	[DllImport("__Internal")]
	private static extern void _NPRequestPauseRecording (String userId);
	[DllImport("__Internal")]
	private static extern void _NPRequestResumeRecording (String userId);
	[DllImport("__Internal")]
	private static extern void _NPRequestRewindRecording (String userId, UInt32 timeDeltaMilliseconds);
	[DllImport("__Internal")]
	private static extern void _NPRequestStopRecording (String userId);
    
	[DllImport("__Internal")]
	private static extern bool _NPConsumeSyncEvent(String objectId, [Out] out _NPSyncEventInfo syncEventInfo);
    [DllImport("__Internal")]
    private static extern bool _NPConsumeCustomMessage(String MessageID, [Out] out _NPTournamentCustomMessage Data);
    [DllImport("__Internal")]
    private static extern void _NPRemoveStoredObjectWithId(String MessageID);
	
	// NB: currently, reliable and unreliable message types are identical, so we keep using the reliable container as our base.
    private static NPTournamentCustomMessageContainer ConsumeCustomMessage(String Id)
    {
        _NPTournamentCustomMessage internalMessage;
        if (!_NPConsumeCustomMessage(Id, out internalMessage))
		{
			return null;
		}
		
		NPTournamentCustomMessageContainer result = new NPTournamentCustomMessageContainer();
		
		// On iOS, Marshal.PtrToStringAnsi uses UTF-8.
		result.PlayerName = Marshal.PtrToStringAnsi(internalMessage.PlayerName);
		result.PlayerID = Marshal.PtrToStringAnsi(internalMessage.PlayerID);
		result.ProfileImageUrl = Marshal.PtrToStringAnsi(internalMessage.ProfilleImageUrl);
		result.PlayerIsBot = internalMessage.PlayerIsBot;
		result.Message = new byte[internalMessage.MessageSize];
		Marshal.Copy(internalMessage.Message, result.Message, 0, internalMessage.MessageSize);
		
        return result;
    }
	
	[DllImport("__Internal")]
	private static extern _NPTournamentStatusInfo _NPConsumeTournamentStatusInfo(String MessageID);
	
    // NextpeerNotSupportedShouldShowError ------------------------------------------------
    [DllImport("__Internal")]
    private static extern void _NPSetNextpeerNotSupportedShouldShowErrors([MarshalAs(UnmanagedType.I1)] Boolean should);
    [DllImport("__Internal")]
    private static extern Boolean _NPGetNextpeerNotSupportedShouldShowErrors();
    
    // Notifications -----------------------------------------------------
	[DllImport("__Internal")]
    private static extern void _NPEnableRankingDisplay([MarshalAs(UnmanagedType.I1)] Boolean EnableRankingDisplay);

	#endregion
	
	#endregion
}

#endif
