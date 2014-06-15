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
        return _NPReleaseVersionString();
    }
	
	public void Init(String GameKey, NPGameSettings? Settings=null)
    {
		// Convenience variable, to save us all the "Value" calls.
		NPGameSettings userSettings = Settings ?? new NPGameSettings();
		
		MSettings settingsVal = new MSettings();
		
		// Setting defaults for unspecified values, to avoid dealing with passing nulls to Objective-C++:
		settingsVal.DisplayName = userSettings.DisplayName ?? "";
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
	
	public void DismissDashboard()
    {
        _NPDismissDashboard();
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
	
	public TimeSpan TimeLeftInTournament()
    {
        return new TimeSpan(0, 0, _NPTimeLeftInTournament());
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
	
	public void PostToFacebookWall(String message, String link, String ImageUrl)
    {
        if (link == null)
            link = "";
        if (ImageUrl == null)
            ImageUrl = "";

        _NPPostToFacebookWallMessage(message, link, ImageUrl);
    }
	
	public NPGamePlayerContainer GetCurrentPlayerDetails()
    {
        return _NPGetCurrentPlayerDetails();
    }
	
	public void OpenFeedDashboard()
    {
        _NPOpenFeedDashboard();
    }
	
	public void EnableRankingDisplay(bool enableRankingDisplay)
	{
		_NPEnableRankingDisplay(enableRankingDisplay);
	}
	
	public void SetAllowInterGameScreen(Boolean allowInterGameScreen)
	{
		_NPSetShouldAllowInterGameScreen(allowInterGameScreen);
	}
	
	public void ResumePlayAgainLogic()
	{
		_NPResumePlayAgainLogic();
	}
	
	public void SetNextpeerNotSupportedShouldShowCustomErrors(Boolean ShowError)
    {
        _NPSetNextpeerNotSupportedShouldShowErrors(ShowError);
    }
	
	public Int32 GetCurrencyAmount()
    {
        return _NPGetCurrencyAmount();
    }
	
	public void SetCurrencyAmount(Int32 amount)
    {
        _NPSetCurrencyAmount(amount);
    }
	
	public void SetSupportsUnifiedCurrency(Boolean supported)
    {
        _NPSwitchUnifiedCurrencySupported(supported);
    }
	
	public NPTournamentStartDataContainer GetTournamentStartData()
    {
		NPTournamentStartDataContainer result = new NPTournamentStartDataContainer();
		
        _NPTournamentStartDataContainer startData = _NPGetTournamentStartData();
		result.TournamentUUID = startData.TournamentUuid;
		result.TournamentName = startData.TournamentName;
		result.TournamentTimeSeconds = startData.TournamentDurationInSeconds;
		result.TournamentRandomSeed = startData.TournamentRandomSeed;
		result.TournamentIsGameControlled = startData.TournamentIsGameControlled;
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
		
		result.PlayerName = player.Name;
		result.PlayerId = player.Id;
		result.PlayerImageUrl = player.ImageUrl;
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
		
		int tournamentPlayerResultsStructSize = Marshal.SizeOf(typeof(_NPTournamentPlayerResults));
		result.SortedResults = new NPTournamentPlayerResults[marshalledStatusInfo.NumberOfResults];
		for (int resultIndex = 0; resultIndex < marshalledStatusInfo.NumberOfResults; resultIndex++)
		{
			_NPTournamentPlayerResults marshalledPlayerResults = 
				(_NPTournamentPlayerResults)Marshal.PtrToStructure(new IntPtr(marshalledStatusInfo.SortedResultsPtr.ToInt64() + tournamentPlayerResultsStructSize*resultIndex),
					typeof(_NPTournamentPlayerResults));
			
			NPTournamentPlayer player = new NPTournamentPlayer() {
				PlayerName = marshalledPlayerResults.PlayerName,
				PlayerId = marshalledPlayerResults.PlayerId,
				PlayerImageUrl = marshalledPlayerResults.PlayerImageUrl,
				PlayerIsBot = marshalledPlayerResults.PlayerIsBot,
				IsCurrentUser = marshalledPlayerResults.PlayerIsCurrentUser
			};
			
			NPTournamentPlayerResults playerResults = new NPTournamentPlayerResults() {
				Player = player,
				IsStillPlaying = marshalledPlayerResults.IsStillPlaying,
				DidForfeit = marshalledPlayerResults.DidForfeit,
				Score = marshalledPlayerResults.Score
			};
			
			result.SortedResults[resultIndex] = playerResults;
		}
		
		return result;
	}
	
	public NPTournamentEndDataContainer GetTournamentResult()
    {
        return _NPGetTournamentResult();
    }
	
	public void RegisterToSyncEvent(string eventName, TimeSpan timeout)
	{
		_NPRegisterToSynchronizedEvent(eventName, timeout.TotalSeconds);
	}
	
	public bool ConsumeSyncEventInfo(string syncEventInfoId, ref string eventName, ref NPSynchronizedEventFireReason fireReason)
	{
		_NPSyncEventInfo syncEventInfo;
		if (!_NPConsumeSyncEvent(syncEventInfoId, out syncEventInfo))
		{
			return false;
		}
		
		eventName = syncEventInfo.EventName;
		fireReason = (NPSynchronizedEventFireReason)syncEventInfo.FireReason;
		
		return true;
	}
	
	#endregion
	
	#region Bridge to Objective C
	
	
	// Marhsalling nullable types (used in NPGameSettings) isn't an option, so we need an intermediate struct to pass the settings to Objective C++.
	private struct MSettings
    {
        public String DisplayName;
        public NPUIInterfaceOrientation NotificationOrientation;
        public Boolean ObserveNotificationOrientationChange;
        public NPNotificationPosition NotificationPosition;
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
		public int MessageSize;
		public IntPtr Message;
		public IntPtr PlayerID;
		public Boolean PlayerIsBot;
	}
	
	private struct _NPTournamentStatusInfo
	{
		public int NumberOfResults;
		public IntPtr SortedResultsPtr;
	}
	
	private struct _NPTournamentPlayerResults
	{
		public String PlayerName;
		public String PlayerId;
		public String PlayerImageUrl;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean PlayerIsBot;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean PlayerIsCurrentUser;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean IsStillPlaying;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean DidForfeit;
		public UInt32 Score;
	}
	
	private struct _NPSyncEventInfo
	{
		public String EventName;
		public int FireReason;
	}
	
	private struct _NPTournamentPlayer
	{
		public String Name;
		public String Id;
		public String ImageUrl;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean IsBot;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean IsCurrentUser;
	}
	
	private struct _NPTournamentStartDataContainer
	{
		public String TournamentUuid;
		public String TournamentName;
		public UInt32 TournamentDurationInSeconds;
		public UInt32 TournamentRandomSeed;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean TournamentIsGameControlled;
		public UInt32 NumberOfPlayers;
		public IntPtr CurrentPlayerPtr;
		public IntPtr OpponentsPtr;
	}
	
	#region PInvoke calls

	[DllImport("__Internal")]
    private static extern string _NPReleaseVersionString();
	
	// Init:
	[DllImport("__Internal")]
    private static extern void _NPInitSettings(String Key, ref MSettings settings);

    // other methods  --------------------------------------------------------------------------------------------------------------------
    [DllImport("__Internal")]
    private static extern void _NPLaunchDashboard();
    [DllImport("__Internal")]
    private static extern void _NPDismissDashboard();
    [DllImport("__Internal")]
    private static extern bool _NPIsNextpeerSupported();
    [DllImport("__Internal")]
    private static extern void _NPPostToFacebookWallMessage(String message, String link, String ImageUrl);
    [DllImport("__Internal")]
    private static extern NPGamePlayerContainer _NPGetCurrentPlayerDetails();
    [DllImport("__Internal")]
    private static extern void _NPPushDataToOtherPlayers(byte[] data, int size);
	[DllImport("__Internal")]
    private static extern void _NPUnreliablePushDataToOtherPlayers(byte[] data, int size);
    [DllImport("__Internal")]
    private static extern void _NPReportScoreForCurrentTournament(UInt32 score);
    [DllImport("__Internal")]
    private static extern bool _NPIsCurrentlyInTournament();
    [DllImport("__Internal")]
    private static extern void _NPReportForfeitForCurrentTournament();
	[DllImport("__Internal")]
    private static extern void _NPReportControlledTournamentOverWithScore(UInt32 score);
    [DllImport("__Internal")]
    private static extern Int32 _NPTimeLeftInTournament();
	[DllImport("__Internal")]
	private static extern void _NPRegisterToSynchronizedEvent(string eventName, double timeout);

    // In memory data access dedicated methods
    // Tournament --------------------------------------------------------
    [DllImport("__Internal")]
    private static extern _NPTournamentStartDataContainer _NPGetTournamentStartData();
    
    [DllImport("__Internal")]
    private static extern NPTournamentEndDataContainer _NPGetTournamentResult();
	
	[DllImport("__Internal")]
	private static extern void _NPResumePlayAgainLogic();
	
	
	[DllImport("__Internal")]
	private static extern void _NPSetShouldAllowInterGameScreen(Boolean allowInterGameScreen);
	

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
    
	[DllImport("__Internal")]
	private static extern bool _NPConsumeSyncEvent(string objectId, [Out] out _NPSyncEventInfo syncEventInfo);
    [DllImport("__Internal")]
    private static extern bool _NPConsumeCustomMessage(String MessageID, [Out] out _NPTournamentCustomMessage Data);
    [DllImport("__Internal")]
    private static extern void _NPRemoveStoredObjectWithId(String MessageID);
	
	// NB: currently, reliable and unreliable message types are identical, so we keep using the reliable container as our base.
    private static NPTournamentCustomMessageContainer ConsumeCustomMessage(String Id)
    {
        _NPTournamentCustomMessage internalMessage = new _NPTournamentCustomMessage();
        int nSizeStruct = Marshal.SizeOf(internalMessage);
        IntPtr pStruct = Marshal.AllocHGlobal(nSizeStruct);
        Marshal.StructureToPtr(internalMessage, pStruct, false);
        if (!_NPConsumeCustomMessage(Id, out internalMessage))
		{
			Marshal.FreeHGlobal(pStruct);
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
		
		Marshal.FreeHGlobal(pStruct);
		
        return result;
    }
	
	[DllImport("__Internal")]
	private static extern _NPTournamentStatusInfo _NPConsumeTournamentStatusInfo(String MessageID);
	
    // NextpeerNotSupportedShouldShowError ------------------------------------------------
    [DllImport("__Internal")]
    private static extern void _NPSetNextpeerNotSupportedShouldShowErrors(Boolean should);
    [DllImport("__Internal")]
    private static extern Boolean _NPGetNextpeerNotSupportedShouldShowErrors();
    
    // Notifications -----------------------------------------------------
	[DllImport("__Internal")]
    private static extern void _NPEnableRankingDisplay(Boolean EnableRankingDisplay);

    // Currency ----------------------------------------------------------
    // Initialise nextpeer with Currencies to be able to use these methods
    // Also, the Game must support Currencies (see your Nextpeer
    // developper account for more informations).
    [DllImport("__Internal")]
    private static extern Int32 _NPGetCurrencyAmount();
    [DllImport("__Internal")]
    private static extern void _NPSetCurrencyAmount(Int32 amount);
    [DllImport("__Internal")]
    private static extern Boolean _NPIsUnifiedCurrencySupported();
    [DllImport("__Internal")]
    private static extern void _NPSwitchUnifiedCurrencySupported(Boolean isSupported);
    [DllImport("__Internal")]
    private static extern void _NPOpenFeedDashboard();
	
	#endregion
	
	#endregion
}

#endif
