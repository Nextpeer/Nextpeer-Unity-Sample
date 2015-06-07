using UnityEngine;
using System.Collections;
using System;

#if UNITY_ANDROID

public class NextpeerAndroid : INextpeer
{
	private AndroidJavaClass _nextpeer;
	
	#region Android-only interface
	
	public void onStart()
	{
		if (isNextpeerInitialized())
		{
			_nextpeer.CallStatic("onStart");
		}
	}
	
	public void onStop(Action forfeitCallback)
	{
		if (isNextpeerInitialized() && IsCurrentlyInTournament())
		{
			ReportForfeitForCurrentTournament();
			
			// Temporary workaround until NP-167 is implemented - when we auto-forfeit the tournament, we must report it to the user.
			forfeitCallback();
		}
	}
	
	public bool isNextpeerInitialized()
	{
		return _nextpeer.CallStatic<bool>("isNextpeerInitialized");
	}
	
	#endregion
	
	public NextpeerAndroid()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			// AndroidJNIHelper.debug = true; // Turn on only during JNI debugging sessions.
			_nextpeer = new AndroidJavaClass("com.nextpeer.unity.NextpeerUnity");
		}
	}
	
	public bool IsNextpeerSupported()
	{
		return _nextpeer.CallStatic<bool>("isNextpeerSupported");
	}

	public string ReleaseVersionString()
	{
		return _nextpeer.CallStatic<string>("getNextpeerVersion");
	}

	public void Init(string GameKey, NPGameSettings? Settings=null)
	{
		if (Settings == null)
		{
			_nextpeer.CallStatic("initialize", GameKey);
		}
		else
		{
			AndroidJavaObject javaSettings = new AndroidJavaObject("com.nextpeer.android.NextpeerSettings");

			if (Settings.Value.NotificationPosition != null)
			{
				AndroidJavaClass javaNotificationPosition = new AndroidJavaClass("com.nextpeer.android.NextpeerSettings$NextpeerRankingDisplayPosition");
				switch (Settings.Value.NotificationPosition.Value)
				{
				case NPNotificationPosition.NPNotificationPosition_TOP:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("TOP"));
					break;
				case NPNotificationPosition.NPNotificationPosition_BOTTOM:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("BOTTOM"));
					break;
				case NPNotificationPosition.NPNotificationPosition_LEFT:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("LEFT"));
					break;
				case NPNotificationPosition.NPNotificationPosition_RIGHT:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("RIGHT"));
					break;
				case NPNotificationPosition.NPNotificationPosition_TOP_LEFT:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("TOP_LEFT"));
					break;
				case NPNotificationPosition.NPNotificationPosition_TOP_RIGHT:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("TOP_RIGHT"));
					break;
				case NPNotificationPosition.NPNotificationPosition_BOTTOM_LEFT:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("BOTTOM_LEFT"));
					break;
				case NPNotificationPosition.NPNotificationPosition_BOTTOM_RIGHT:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationPosition", javaNotificationPosition.GetStatic<AndroidJavaObject>("BOTTOM_RIGHT"));
					break;
				default:
					break;
				}
			}

			if (Settings.Value.RankingDisplayStyle != null)
			{
				AndroidJavaClass javaNotificationStyle = new AndroidJavaClass("com.nextpeer.android.NextpeerSettings$NextpeerRankingDisplayStyle");
				switch (Settings.Value.RankingDisplayStyle.Value)
				{
				case NPRankingDisplayStyle.List:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationStyle", javaNotificationStyle.GetStatic<AndroidJavaObject>("LIST"));
					break;
				case NPRankingDisplayStyle.Solo:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationStyle", javaNotificationStyle.GetStatic<AndroidJavaObject>("SOLO"));
					break;
				default:
					break;
				}
			}

			if (Settings.Value.RankingDisplayAlignment != null)
			{
				AndroidJavaClass javaNotificationAlignment = new AndroidJavaClass("com.nextpeer.android.NextpeerSettings$NextpeerRankingDisplayAlignment");
				switch (Settings.Value.RankingDisplayAlignment.Value)
				{
				case NPRankingDisplayAlignment.Horizontal:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationAlignment", javaNotificationAlignment.GetStatic<AndroidJavaObject>("HORIZONTAL"));
					break;
				case NPRankingDisplayAlignment.Vertical:
					javaSettings.Set<AndroidJavaObject>("inGameNotificationAlignment", javaNotificationAlignment.GetStatic<AndroidJavaObject>("VERTICAL"));
					break;
				default:
					break;
				}
			}

			// shouldShowStatusBar will be set to false in Unity.

			_nextpeer.CallStatic("initialize", GameKey, javaSettings);
		}
	}

	public void LaunchDashboard()
	{
		_nextpeer.CallStatic("launch");
	}

	public bool IsCurrentlyInTournament()
	{
		return _nextpeer.CallStatic<bool>("isCurrentlyInTournament");
	}

	public void ReportScoreForCurrentTournament(UInt32 score)
	{
		_nextpeer.CallStatic("reportScoreForCurrentTournament", (int)score);
	}

	public void ReportControlledTournamentOverWithScore(UInt32 score)
	{
		_nextpeer.CallStatic("reportControlledTournamentOverWithScore", (int)score);
	}

	public void ReportForfeitForCurrentTournament()
	{
		_nextpeer.CallStatic("reportForfeitForCurrentTournament");
	}

	public void PushDataToOtherPlayers(byte[] data)
	{
		_nextpeer.CallStatic("pushDataToOtherPlayers", data);
	}

	public void UnreliablePushDataToOtherPlayers(byte[] data)
	{
		_nextpeer.CallStatic("unreliablePushDataToOtherPlayers", data);
	}

	public void AddWhitelistTournament(string tournamentId)
	{
		_nextpeer.CallStatic("addWhitelistTournament", tournamentId);
	}

	public void RemoveWhitelistTournament(string tournamentId)
	{
		_nextpeer.CallStatic("removeWhitelistTournament", tournamentId);
	}

	public void ClearTournamentWhitelist()
	{
		_nextpeer.CallStatic("clearTournamentWhitelist");
	}

	public void AddBlacklistTournament(string tournamentId)
	{
		_nextpeer.CallStatic("addBlacklistTournament", tournamentId);
	}

	public void RemoveBlacklistTournament(string tournamentId)
	{
		_nextpeer.CallStatic("removeBlacklistTournament", tournamentId);
	}

	public void ClearTournamentBlacklist()
	{
		_nextpeer.CallStatic("clearTournamentBlacklist");
	}

	public NPGamePlayerContainer GetCurrentPlayerDetails()
	{
		AndroidJavaObject javaPlayer = _nextpeer.CallStatic<AndroidJavaObject>("getCurrentPlayerDetails");
		NPGamePlayerContainer result = new NPGamePlayerContainer();

		result.PlayerName = javaPlayer.Get<string>("playerName");
		result.ProfileImageURL = javaPlayer.Get<string>("playerImageUrl");
		result.PlayerId = javaPlayer.Get<string>("playerId");

		return result;
	}
	
	public void EnableRankingDisplay(bool enableRankingDisplay)
	{
		_nextpeer.CallStatic("enableRankingDisplay", enableRankingDisplay);
	}

	public void SetNextpeerNotSupportedShouldShowCustomErrors(Boolean ShowError)
	{
	}

	public NPTournamentStartDataContainer GetTournamentStartData()
	{
		AndroidJavaObject javaStartData = _nextpeer.CallStatic<AndroidJavaObject>("getTournamentStartData");
		NPTournamentStartDataContainer result = new NPTournamentStartDataContainer();

		result.TournamentUUID = javaStartData.Get<string>("tournamentUuid");
		result.TournamentRandomSeed = unchecked( (uint)javaStartData.Get<int>("tournamentRandomSeed") );
		result.TournamentName = javaStartData.Get<string>("tournamentName");
		result.TournamentNumberOfPlayers = (uint)javaStartData.Get<int>("numberOfPlayers");
		result.CurrentPlayer = GetTournamentPlayer(javaStartData.Get<AndroidJavaObject>("currentPlayer"));
		
		AndroidJavaObject javaOpponents = javaStartData.Get<AndroidJavaObject>("opponents");
		result.Opponents = new NPTournamentPlayer[result.TournamentNumberOfPlayers - 1];
		for (int opponentIndex = 0; opponentIndex < result.TournamentNumberOfPlayers - 1; opponentIndex++)
		{
			result.Opponents[opponentIndex] = GetTournamentPlayer(javaOpponents.Call<AndroidJavaObject>("get", opponentIndex));
		}

		return result;
	}

	public NPTournamentCustomMessageContainer ConsumeReliableCustomMessage(String id)
	{
		return ConsumeCustomMessage<NPTournamentCustomMessageContainer>(id);
	}

	public NPTournamentUnreliableCustomMessageContainer ConsumeUnreliableCustomMessage(String id)
	{
		return ConsumeCustomMessage<NPTournamentUnreliableCustomMessageContainer>(id);
	}

	private TMessage ConsumeCustomMessage<TMessage>(String messageKey)
		where TMessage : NPTournamentCustomMessageBase, new()
	{
		AndroidJavaObject javaMessage = null;
		try
		{
			javaMessage = _nextpeer.CallStatic<AndroidJavaObject>("retrieveObjectWithId", messageKey);
		}
		catch (Exception e)
		{
			// Probably the object is not there and we got "null" from Java.
			return null;
		}
		
		TMessage result = new TMessage();

		result.Message = javaMessage.Get<byte[]>("customMessage");
		result.PlayerID = javaMessage.Get<string>("playerId");
		result.ProfileImageUrl = javaMessage.Get<string>("playerImageUrl");
		result.PlayerIsBot = javaMessage.Get<bool>("playerIsBot");
		result.PlayerName = javaMessage.Get<string>("playerName");
		
		return result;
	}

	public NPTournamentStatusInfo? ConsumeTournamentStatusInfo(String messageKey)
	{
		AndroidJavaObject javaTournamentStatus = null;
		try
		{
			javaTournamentStatus = _nextpeer.CallStatic<AndroidJavaObject>("retrieveObjectWithId", messageKey);
		}
		catch (Exception e)
		{
			// Probably the object is not there and we got "null" from Java.
			return null;
		}

		AndroidJavaObject javaSortedResults = javaTournamentStatus.Get<AndroidJavaObject>("sortedResults");
		int resultsCount = javaSortedResults.Call<int>("size");
		NPTournamentPlayerResults[] sortedResults = new NPTournamentPlayerResults[resultsCount];
		for (int resultsIndex = 0; resultsIndex < resultsCount; resultsIndex++)
		{
			AndroidJavaObject javaPlayerResult = javaSortedResults.Call<AndroidJavaObject>("get", resultsIndex);
			AndroidJavaObject javaPlayer = javaPlayerResult.Get<AndroidJavaObject>("player");

			NPTournamentPlayerResults playerResult = new NPTournamentPlayerResults();
			playerResult.Player = GetTournamentPlayer(javaPlayer);
			playerResult.DidForfeit = javaPlayerResult.Get<bool>("didForfeit");
			playerResult.IsStillPlaying = javaPlayerResult.Get<bool>("isStillPlaying");
			playerResult.Score = (uint)javaPlayerResult.Get<int>("score");

			sortedResults[resultsIndex] = playerResult;
		}

		NPTournamentStatusInfo result = new NPTournamentStatusInfo();
		result.SortedResults = sortedResults;

		return result;
	}

	public void RemoveStoredObjectWithId(String MessageID)
	{
		_nextpeer.CallStatic("removeStoredObjectWithId", MessageID);
	}

	public NPTournamentEndDataContainer GetTournamentResult()
	{
		AndroidJavaObject javaEndData = _nextpeer.CallStatic<AndroidJavaObject>("getTournamentResult");
		NPTournamentEndDataContainer result = new NPTournamentEndDataContainer();

		result.TournamentUUID = javaEndData.Get<string>("tournamentUuid");
		result.TournamentTotalPlayers = (uint)javaEndData.Get<int>("tournamentTotalPlayers");

		AndroidJavaObject javaCurrentPlayer = javaEndData.Get<AndroidJavaObject>("currentPlayer");
		result.PlayerName = javaCurrentPlayer.Get<string>("playerName");

		return result;
	}
	
	public void RegisterToSyncEvent(string eventName, TimeSpan timeout)
	{
		_nextpeer.CallStatic("registerToSynchronizedEvent", eventName, (int)timeout.TotalSeconds);
	}
	
	public void CaptureMoment()
	{
		_nextpeer.CallStatic("captureMoment");
	}
	
	public bool ConsumeSyncEventInfo(string syncEventInfoId, ref string eventName, ref NPSynchronizedEventFireReason fireReason)
	{
		AndroidJavaObject javaMessage = null;
		try
		{
			javaMessage = _nextpeer.CallStatic<AndroidJavaObject>("retrieveObjectWithId", syncEventInfoId);
		}
		catch (Exception e)
		{
			// Probably the object is not there and we got "null" from Java.
			return false;
		}
		
		eventName = javaMessage.Call<String>("getEventName");
		
		AndroidJavaObject javaFireReason = javaMessage.Call<AndroidJavaObject>("getFireReason");
		
		AndroidJavaClass javaFireReasonEnum = new AndroidJavaClass("com.nextpeer.android.NextpeerSynchronizedEventFire");
		if (javaFireReason.Call<bool>("equals", javaFireReasonEnum.GetStatic<AndroidJavaObject>("ALL_REACHED")))
		{
			fireReason = NPSynchronizedEventFireReason.AllReached;
		}
		else if (javaFireReason.Call<bool>("equals", javaFireReasonEnum.GetStatic<AndroidJavaObject>("ALREADY_FIRED")))
		{
			fireReason = NPSynchronizedEventFireReason.AlreadyFired;
		}
		else if (javaFireReason.Call<bool>("equals", javaFireReasonEnum.GetStatic<AndroidJavaObject>("TIMEOUT")))
		{
			fireReason = NPSynchronizedEventFireReason.Timeout;
		}
		
		return true;
	}
	
	private NPTournamentPlayer GetTournamentPlayer(AndroidJavaObject javaPlayer)
	{
		NPTournamentPlayer player = new NPTournamentPlayer();
		
		player.IsCurrentUser = javaPlayer.Call<bool>("isCurrentUser");
		player.PlayerIsBot = javaPlayer.Get<bool>("playerIsBot");
		player.PlayerId = javaPlayer.Get<string>("playerId");
		player.PlayerImageUrl = javaPlayer.Get<string>("playerImageUrl");
		player.PlayerName = javaPlayer.Get<string>("playerName");
		
		return player;
	}

	//Recording manipulation
	public void ReportScoreModifier (String userId, Int32 scoreModifier){
		_nextpeer.CallStatic("reportScoreModifier", userId, scoreModifier);
	}
	public void RequestFastForwardRecording (String userId, TimeSpan timeDelta){
		_nextpeer.CallStatic("requestFastForwardRecording", userId, (int)timeDelta.TotalMilliseconds);
	}
	public void RequestPauseRecording (String userId){
		_nextpeer.CallStatic("requestPauseRecording", userId);
	}
	public void RequestResumeRecording (String userId){
		_nextpeer.CallStatic("requestResumeRecording", userId);
	}
	public void RequestRewindRecording(String userId, TimeSpan timeDelta){
		_nextpeer.CallStatic("requestRewindRecording", userId, (int)timeDelta.TotalMilliseconds);
	}
	public void RequestStopRecording(String userId){
		_nextpeer.CallStatic("requestStopRecording", userId);		
	}

    public bool FrameStart(int width, int height)
    {
		return _nextpeer.CallStatic<bool>("frameStart", width, height);
    }
        
	public void FrameEnd()
    {
		_nextpeer.CallStatic("frameEnd");
    }
}

#endif
