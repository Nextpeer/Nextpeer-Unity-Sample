using UnityEngine;
using System.Collections;
using System.Globalization;
using System;
using System.IO;
using System.Collections.Generic;
using Object = System.Object;
using UObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_IPHONE || UNITY_ANDROID

#region Structures

/// <summary>
/// The NPGamePlayerContainer can be used to retrieve data on the current player.
/// </summary>
public struct NPGamePlayerContainer
{
	/// <summary>
	/// The player identifier (also see NPTournamentPlayer.PlayerId).
	/// </summary>
	public String PlayerId;
	
	/// <summary>
	/// The name of the player.
	/// </summary>
    public String PlayerName;
	
	/// <summary>
	/// The player image URL.
	/// </summary>
    public String ProfileImageURL;
}

/// <summary>
/// NPTournamentPlayer represents a player in a tournament (may or not may not be the current user).
/// </summary>
public struct NPTournamentPlayer
{
	/// <summary>
	/// The player name.
	/// </summary>
	public String PlayerName;
	
	/// <summary>
	/// A unique and persistent identifier for the player in Nextpeer.
	/// </summary>
	public String PlayerId;
	
	/// <summary>
	/// The player's avatar image URL.
	/// </summary>
	public String PlayerImageUrl;
	
	/// <summary>
	/// A flag specifying if the player is a bot (a recording of a previous game) or a real player.
	/// </summary>
	public Boolean PlayerIsBot;
	
	/// <summary>
	/// A flag specifying if the struct represents the current user.
	/// </summary>
	public Boolean IsCurrentUser;
}

/// <summary>
/// NPTournamentPlayerResults represents the tournament results for a given player.
/// </summary>
public struct NPTournamentPlayerResults
{
	/// <summary>
	/// The player for which this object reports the results.
	/// </summary>
	public NPTournamentPlayer Player;
	
	/// <summary>
	/// Marks if the player is still in the game.
	/// </summary>
	public Boolean IsStillPlaying;
	
	/// <summary>
	/// Marks if the player forfeit the tournament. This can happen, for example, when the player exits mid-tournament in a timed tournament.
	/// </summary>
	public Boolean DidForfeit;
	
	/// <summary>
	/// The score of the player.
	/// </summary>
	public UInt32 Score;
}

/// <summary>
/// NPTournamentStatusInfo represents the current tournament status - the ranks and scores of the participating players.
/// </summary>
public struct NPTournamentStatusInfo
{
	/// <summary>
	/// An array of NPTournamentPlayerResults objects, sorted by player ranks in ascending order (player at index 0 is ranked first).
	/// </summary>
	public NPTournamentPlayerResults[] SortedResults;
}

/// <summary>
/// The base class for all custom messages. Used in messages from both reliable and unreliable communication channels.
/// Carries the payload and context information - the sender's ID, name, image URL, etc.
/// </summary>
public abstract class NPTournamentCustomMessageBase
{
	/// <summary>
	/// The name of the player.
	/// </summary>
    public String PlayerName { get; internal set; }
	
	/// <summary>
	/// The player's profile image URL.
	/// </summary>
    public String ProfileImageUrl { get; internal set; }
	
	/// <summary>
	/// The custom message (passed as a byte buffer).
	/// </summary>
    public byte[] Message { get; internal set; }
	
	/// <summary>
	/// A unique player identifier for the current game.
	/// </summary>
	public String PlayerID { get; internal set; }
	
	/// <summary>
	/// Boolean value that indicates if this message came form a bot recording or a real-life player.
	/// </summary>
	public Boolean PlayerIsBot { get; internal set; }
}

/// <summary>
/// NPTournamentCustomMessageContainer is the message container for the reliable communication channel.
/// </summary>
public class NPTournamentCustomMessageContainer : NPTournamentCustomMessageBase
{

}

/// <summary>
/// NPTournamentUnreliableCustomMessageContainer is the message container for the unreliable communication channel.
/// </summary>
public class NPTournamentUnreliableCustomMessageContainer : NPTournamentCustomMessageBase
{
	
}

/// <summary>
/// Your application's display name. Used as the game title (welcome screen, tournament selector, etc.).
/// 
/// If set, will override the display name value from the developer dashboard.
/// </summary>
public struct NPGameSettings
{
	/// <summary>
	/// Defines the in-game notification position.
	/// 
	/// Default: NPNotificationPosition_TOP
	/// </summary>
	public NPNotificationPosition? NotificationPosition;
	
	/// <summary>
	/// Specifies the style for the in-game ranking display.
	/// 
	/// Default: NPRankingDisplayList
	/// </summary>
	public NPRankingDisplayStyle? RankingDisplayStyle;

	/// <summary>
	/// Specifies the alignment of the in-game ranking display. This setting only has effect if the ranking display style is set to NPRankingDisplayList.
	/// 
	/// Default: NPRankingDisplayAlignmentHorizontal
	/// </summary>
	public NPRankingDisplayAlignment? RankingDisplayAlignment;

#if UNITY_IPHONE	
	/// <summary>
	/// Defines what orientation the game notification should appear. The notification system does not auto rotate.
	/// 
	/// Default: UIInterfaceOrientationPortrait
	/// </summary>
    public NPUIInterfaceOrientation? NotificationOrientation;
	
	/// <summary>
	/// Defines if Nextpeer should observe notification orientation change and adjust the in-game notifications according to changes.
	/// For example, if your game supports Landcape orientation, Default UIInterfaceOrientationLandscapeRight, you can verify that even if the user will
	/// change the orientation (in-game) to UIInterfaceOrientationLandscapeLeft the notification orientation will adjust it self.
	/// 
	/// Default: false
	/// </summary>
    public Boolean? ObserveNotificationOrientationChange;
	
	/// <summary>
	/// Defines if Nextpeer should observe device orientation change and adjust the dashboard according to changes.
	/// Nextpeer will keep the transformation in the main orientation. For example if you game supports landscape orientation,
	/// Nextpeer will switch between LandscapeLeft to LandscapeRight (but will not switch to portrait).
	/// 
	/// Default: false
	/// </summary>
    public Boolean? SupportsDashboardRotation;
	
	/// <summary>
	/// Defines the orientation in which the Nextpeer dashboard will be first launched. If not specified, Nextpeer will try to orient itself according to the status bar orientation.
	/// 
	/// Default: [UIApplication sharedApplication].statusBarOrientation
	/// </summary>
	public NPUIInterfaceOrientation? InitialDashboardOrientation;

	/// <summary>
	/// Specifies the style of the animation of the in-game ranking display. This setting can be set to optimise performance.
	/// 
	/// Default: NPRankingDisplayAnimationOptimised
	/// </summary>
	public NPRankingDisplayAnimationStyle? RankingDisplayAnimationStyle;
#endif
}

/// <summary>
/// The NPTournamentStartDataContainer contains information about the tournament which is about to be played.
/// </summary>
public struct NPTournamentStartDataContainer
{
	/// <summary>
	/// The tournament UUID is provided so that your game can identify which tournament needs to be loaded.
	/// You can find the UUID in the developer dashboard.
	/// </summary>
    public String TournamentUUID;
	
	/// <summary>
	/// The tournament display name.
	/// </summary>
    public String TournamentName;
	
	/// <summary>
	/// A random seed generated for this tournament. All players within the same tournament
	/// receive the same seed from the tournament. Can be used for level generation, to ensure
	/// all players play the same level in a specific game.
	/// </summary>
    public UInt32 TournamentRandomSeed;
	
	/// <summary>
	/// The number of players that started this tournament. Includes the current player.
	/// </summary>
	public UInt32 TournamentNumberOfPlayers;
	
	/// <summary>
	/// The current player.
	/// </summary>
	public NPTournamentPlayer CurrentPlayer;
	
	/// <summary>
	/// The opponents.
	/// </summary>
	public NPTournamentPlayer[] Opponents;
}

/// <summary>
/// The NPTournamentEndDataContainer contains information about the tournament that just ended.
/// </summary>
public struct NPTournamentEndDataContainer
{
	/// <summary>
	/// The tournament UUID is provided so that your game can identify which tournament needs to be loaded.
	/// You can find the UUID in the developer dashboard.
	/// </summary>
    public String TournamentUUID;
	
	/// <summary>
	/// The name of the player.
	/// </summary>
    public String PlayerName;
	
	/// <summary>
	/// The number of players in the tournament.
	/// </summary>
    public UInt32 TournamentTotalPlayers;

#if UNITY_IPHONE	
	/// <summary>
	/// The player rank in the tournament (where 1 means first, 1..tournamentTotalPlayers).
	/// </summary>
	public UInt32 PlayerRankInTournament;

	/// <summary>
	/// The player's score at the end of the tournament.
	/// </summary>
    public UInt32 PlayerScore;
#endif
}
#endregion

#region Enums

/// <summary>
/// The interface orientation, ported from Cocoa.
/// </summary>
public enum NPUIInterfaceOrientation
{
	Portrait = 1,
	PortraitUpsideDown = 2,
	LandscapeLeft = 4,
	LandscapeRight = 3,
}

/// <summary>
/// Defines where in-game notifications (including the in-game display ranking) can appear on the screen.
/// </summary>
public enum NPNotificationPosition
{
	/// <summary>
	/// Rankings are shown in the top-center of the screen
	/// </summary>
    NPNotificationPosition_TOP = 0,
	
	/// <summary>
	/// Rankings are shown in the bottom-center of the screen
	/// </summary>
    NPNotificationPosition_BOTTOM,

	/// <summary>
	/// Rankings are shown in the top-left of the screen
	/// </summary>
    NPNotificationPosition_TOP_LEFT,
	
	/// <summary>
	/// Rankings are shown in the bottom-left of the screen
	/// </summary>
    NPNotificationPosition_BOTTOM_LEFT,
	
	/// <summary>
	/// Rankings are shown in the top-right of the screen
	/// </summary>
    NPNotificationPosition_TOP_RIGHT,
	
	/// <summary>
	/// Rankings are shown in the bottom-right of the screen
	/// </summary>
    NPNotificationPosition_BOTTOM_RIGHT,
	
	/// <summary>
	/// Rankings are shown in the middle-left of the screen
	/// </summary>
	NPNotificationPosition_LEFT,
	
	/// <summary>
	/// Rankings are shown in the middle-right of the screen
	/// </summary>
	NPNotificationPosition_RIGHT
}

/// <summary>
/// Defines the possible styles for the in-game ranking display.
/// </summary>
public enum NPRankingDisplayStyle
{
	/// <summary>
	/// Displays the ranks as a list of 2 or 3 players, centered on the current player, who is flanked by the players immediately above and below him in rank.
	/// </summary>
	List = 0,
	
	/// <summary>
	/// Displays only the current player's avatar, and a label which indicates the rank of the current player relative to all the tournament participants. Does not show any other player.
	/// </summary>
	Solo
}

/// <summary>
/// Defines the animation style for the in-game ranking display.
/// </summary>
public enum NPRankingDisplayAnimationStyle
{
	/// <summary>
	/// Optimised animation, based on the current device. Older devices will have reduced animaiton, to prevent a negative impact on performance.
	/// </summary>
	Optimised = 0,
	
	/// <summary>
	/// Full animation (on all devices). In some cases this may negatively affect performance, particularly on older devices.
	/// </summary>
	Full,
	
	/// <summary>
	/// No animation (on all devices). Use this if testing shows that the optimised animation style negatively affects performance.
	/// </summary>
	None
}

/// <summary>
/// Defines the alignment of the in-game ranking display.
/// </summary>
public enum NPRankingDisplayAlignment
{
	/// <summary>
	/// Horizontal alignment - avatars are aligned left-to-right.
	/// </summary>
	Horizontal = 0,
	
	/// <summary>
	/// Vertical alignment - avatars are aligned top-down.
	/// </summary>
	Vertical
}

/// <summary>
/// The reason for firing a synchronized event.
/// </summary>
public enum NPSynchronizedEventFireReason
{
	/// <summary>
	/// All participants have registered for the event.
	/// </summary>
	AllReached = 1,
	
	/// <summary>
	/// The registration timeout was reached before all participants registered for the event (at least one participant didn't register for the event).
	/// </summary>
	Timeout,
	
	/// <summary>
	/// The synchronized event was already fired before the latest registration attempt was made.
	/// </summary>
	AlreadyFired
}

#endregion

public class Nextpeer : MonoBehaviour
{
	#region Public interface

	private static Nextpeer Instance;
	
	#region Events
	
	// Events corresponding to NextpeerDelegate:
	
	/// <summary>
	/// Occurs before the tournament will start (prior the dismissing the dashboard).
	/// </summary>
	public static event Action<NPTournamentStartDataContainer> WillTournamentStartWithDetails;
	
	/// <summary>
	/// Occurs when a tournament is about to start.
	/// The tournament start container will give you some details on the tournament which is about to be played.
	/// For example the tournament UUID, name and time.
	/// </summary>
	public static event Action<NPTournamentStartDataContainer> DidTournamentStartWithDetails;
	
	/// <summary>
	/// Occurs when the current tournament has finished.
	/// In here you can place some cleanup code. For example,
	/// you can use this method to recycle the game scene.
	/// </summary>
	public static event Action DidTournamentEnd;
	
	/// <summary>
	/// Occurs immediately before the Nextpeer dashboard will appear. You should stop your animations here.
	/// </summary>
	public static event Action DashboardWillAppear;
	
	/// <summary>
	/// Occurs when the dashboard has finished its animated transition and is now fully visible.
	/// </summary>
	public static event Action DashboardDidAppear;
	
	/// <summary>
	/// Occurs immediately before the dashboard will disappear.
	/// </summary>
	public static event Action DashboardWillDisappear;
	
	/// <summary>
	/// Occurs immediately after the dashboard will disappear.
	/// </summary>
	public static event Action DashboardDidDisappear;
	
	/// <summary>
	/// Occurs when the player closes Nextpeer's dashboard and returns to the game.
	/// </summary>
	public static event Action DashboardDidReturnToGame;
	
	// Events corresponding to TournamentDelegate:
	
	/// <summary>
	/// Occurs when Nextpeer has received a buffer from another player via the reliable communication channel.
	/// You can use these buffers to create custom notifications and events while engaging the other players
	/// that are currently playing. The container that is passed contains the sending user's name and image as well
	/// as the message being sent.
	/// </summary>
	public static event Action<NPTournamentCustomMessageContainer> DidReceiveTournamentCustomMessage;
	
	/// <summary>
	/// Occurs when Nextpeer has received a buffer from another player via the unreliable communication channel.
	/// </summary>
	public static event Action<NPTournamentUnreliableCustomMessageContainer> DidReceiveUnreliableTournamentCustomMessage;
	
	/// <summary>
	/// Occurs when the current tournament has finished and the platform gathered the information from all the players.
	/// It might take some time between firing the Nextpeer.DidTournamentEnd event and this event,
	/// as the platform needs to wait for the last result from each player.
	/// </summary>
	public static event Action<NPTournamentEndDataContainer> DidReceiveTournamentResults;
	
	/// <summary>
	/// Occurs when a synchronized event was fired.
	/// 
	/// The event will only be fired if you registered for it beforehand with RegisterToSynchronizedEvent().
	/// 
	/// The first parameter of the Action is the synchronized event name, the second is the reason that the event was fired.
	/// </summary>
	public static event Action<string, NPSynchronizedEventFireReason> DidReceiveSynchronizedEvent;

	// Events corresponding NotificationDelegate:

	/// <summary>
	/// Occurs when a tournament status is reported. A tournament status includes information
	/// regarding all the players in the tournament - their ranks, who is still playing, their names, IDs, avatar images, etc.
	/// This information can be used to, for example, generate custom ranking notifications.
	/// 
	/// The rate of tournament status updates is approximately one per second.
	/// </summary>
	public static event Action<NPTournamentStatusInfo> DidReceiveTournamentStatus;

	#endregion
	
	#region Methods
	
	#region General
	
	/// <summary>
	/// Call this method to verify if the current runtime environment supports Nextpeer requirements.
	/// 
	/// Minimum iOS version supported by the SDK is iOS 4.3. Initialising Nextpeer when it's not supported will display an error.
	/// To disable the error, call Nextpeer.SetNextpeerNotSupportedShouldShowCustomErrors() BEFORE calling Nextpeer.Init.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the runtime requirements match, <c>false</c> otherwise.
	/// </returns>
	public static bool IsNextpeerSupported()
    {
#if UNITY_EDITOR
		return false;
#endif
		
		return getINextpeerInstance().IsNextpeerSupported();
	}
	
	/// <returns>
	/// The release Version String of the Nextpeer client library in use.
	/// </returns>
	public static string ReleaseVersionString()
    {
        return getINextpeerInstance().ReleaseVersionString();
    }
	
	/// <summary>
	/// Initialise Nextpeer. You should call this method as early as possible within your game.
	/// </summary>
	/// <param name='GameKey'>
	/// The game key for the game's bundle ID, as specified in the Nextpeer website dashboard.
	/// </param>
	/// <param name='Settings'>
	/// Settings to customise Nextpeer. Optional.
	/// </param>
	public static void Init(String GameKey, NPGameSettings? Settings=null)
    {
#if UNITY_EDITOR
		return;
#endif
		//-------Lazy initialization ----//
		//Here we are looking for Nextpeer gameObject before we create it, for backward compatibility -
		//to handle cases of users which created it manually (Probably before NU-44 was fixed).
		Instance = (Nextpeer)FindObjectOfType(typeof(Nextpeer));
		if (Instance == null)
		{ 	
			Instance = (new GameObject("Nextpeer")).AddComponent<Nextpeer>(); 
		}
		//-------------------------------//

		if (_isInitialised)
		{
			return;
		}
		
		getINextpeerInstance().Init (GameKey, Settings);
		
		_isInitialised = true;
    }
	
	/// <summary>
	/// Launches the Nextpeer dashboard.
	/// 
	/// If Nextpeer.IsNextpeerSupported returns NO this method will do nothing.
	/// </summary>
	public static void LaunchDashboard()
    {
#if UNITY_EDITOR
		EditorUtility.DisplayDialog("Nextpeer error", "Nextpeer not supported in the Editor, please use a device or a simulator/emulator.", "OK");
		return;
#endif
		
        getINextpeerInstance().LaunchDashboard();
    }
	
	#endregion
	
	#region Tournament
	
	/// <summary>
	/// Call this method to check if a tournament is running at the moment.
	/// </summary>
	/// <returns>
	/// <c>true</c> if there is an active tournament; otherwise, <c>false</c>.
	/// </returns>
	public static bool IsCurrentlyInTournament()
    {
        return getINextpeerInstance().IsCurrentlyInTournament();
    }
	
	/// <summary>
	/// Call this method to report the current score for the tournament. This allows Nextpeer to send
 	/// various notifications about the players' scores.
	/// </summary>
	/// <param name='score'>
	/// The current player's score.
	/// </param>
	public static void ReportScoreForCurrentTournament(UInt32 score)
    {
        getINextpeerInstance().ReportScoreForCurrentTournament(score);
    }
	
	/// <summary>
	/// Call this method when your game manages the current tournament and the player just died (a.k.a. 'Last Man Standing').
	/// Nextpeer will call the Nextpeer.DidTournamentEnd event after reporting the last score.
	///
	/// The method will act only if the current tournament is of a 'GameControlled' type.
	/// </summary>
	/// <param name='score'>
	/// The final score of the player in this tournament.
	/// </param>
	public static void ReportControlledTournamentOverWithScore(UInt32 score)
    {
        getINextpeerInstance().ReportControlledTournamentOverWithScore(score);
    }
	
	/// <summary>
	/// Call this method when the user wishes to exit the current tournament prematurely.
	/// Note that this will count as a loss for the player, and will not re-launch the Nextpeer dashboard.
	/// To finish the tournament under regular conditions (e.g., player died in-game), use Nextpeer.ReportControlledTournamentOverWithScore.
	/// This will close any in-game notifiactions and dialogs.
	/// </summary>
	public static void ReportForfeitForCurrentTournament()
    {
        getINextpeerInstance().ReportForfeitForCurrentTournament();
    }

	/// <summary>
	/// This method is used to push a byte buffer to the other players over a reliable communication channel.
	/// 
	/// This can be used to create custom notifications or some other interactive mechanism
	/// that incorporates the other players. The buffer will be sent to the other players and will fire the
	/// Nextpeer.DidReceiveTournamentCustomMessage event in their game.
	/// 
	/// For best performance, make sure not to call this method too frequently, and don't send large buffers.
	/// Otherwise, the packets will be throttled (you will receive a warning about this in the console).
	/// </summary>
	/// <param name='data'>
	/// The byte buffer to send to the other palyers.
	/// </param>
	public static void PushDataToOtherPlayers(byte[] data)
    {
        getINextpeerInstance().PushDataToOtherPlayers(data);
    }
	
	/// <summary>
	/// This method is used to push a byte buffer to the other players through an unreliable communication channel.
	/// While order will be preserved, packets may be dropped. The advantage of this channel over the reliable channel is its low latency.
	/// 
	/// This can be used to send information that is not critical, but becomes stale very fast, such as other player's positions.
	/// 
	/// The payload of this method is limited to about 1300 bytes.
	/// </summary>
	/// <param name='data'>
	/// The byte buffer to send to the other palyers.
	/// </param>
	public static void UnreliablePushDataToOtherPlayers(byte[] data)
    {
        getINextpeerInstance().UnreliablePushDataToOtherPlayers(data);
    }

	/// <summary>
	///  Registers to a synchronized event.
 	///
	///  A synchronized event can be used to synchronize all players at a particular point in the game. For example, at the beginning of the game, each client may need to load resources, which takes variable time, depending on the player's device. The event will be fired (see the DidReceiveSynchronizedEvent event) either when everyone registered for it, or after the specified timeout, and all players will receive it at the same time.
 	///
	///  When working with synchronized events, you should be aware of the following edge cases:
 	///  1. Clients who are too late to register to an event will not be notified of the event until they register.
	///  2. Recordings will pause when they register to an event that wasn't registered to by a real player, and will wait until the event is fired due the registration of a live player(s).
 	///  3. Recordings that are late to register to an event will behave just as regular clients, and will continue their playback as usual.
	/// </summary>
	/// <param name='eventName'>
	/// The name of the synchronized event to register to.
	/// </param>
	/// <param name='timeout'>
	/// The maximum amount of time to wait for all other participants to register for the synchronized event.
	/// </param>
	public static void RegisterToSynchronizedEvent(string eventName, TimeSpan timeout)
	{
		getINextpeerInstance().RegisterToSyncEvent(eventName, timeout);
	}

	/// <summary>
	/// Define which moments to capture. Must be called during a tournament.
	/// On calling captureMoment() during a tournament, 
	/// no other moments will captured by Nextpeer during the same tournament.
	/// </summary>
	public static void CaptureMoment()
	{
		getINextpeerInstance().CaptureMoment();
	}
	
	#region Tournament white/black lists
	
	/// <summary>
	/// Adds a tournament to the whitelist of accepted tournaments. All tournaments not in the whitelist will be ignored by Nextpeer.
	/// 
	/// If no whitelist or blacklist is set, all tournaments will be accepted.
	/// </summary>
	/// <param name='tournamentId'>
	/// The touranment ID to whitelist, as given in the Nextpeer website dashboard.
	/// </param>
	public static void AddWhitelistTournament(string tournamentId)
	{
		getINextpeerInstance().AddWhitelistTournament(tournamentId);
	}
	
	/// <summary>
	/// Removes the tournament from the whitelist.
	/// </summary>
	/// <param name='tournamentId'>
	/// The tournament ID to remove from the whitelist.
	/// </param>
	public static void RemoveWhitelistTournament(string tournamentId)
	{
		getINextpeerInstance().RemoveWhitelistTournament(tournamentId);
	}
	
	/// <summary>
	/// Clears the tournament whitelist.
	/// </summary>
	public static void ClearTournamentWhitelist()
	{
		getINextpeerInstance().ClearTournamentWhitelist();
	}
	
	/// <summary>
	/// Adds a tournament to the blacklist of rejected tournaments. All tournaments not in the blacklist will be accepted by Nextpeer.
	/// 
	/// If no whitelist or blacklist is set, all tournaments will be accepted.
	/// </summary>
	/// <param name='tournamentId'>
	/// The touranment ID to blacklist, as given in the Nextpeer website dashboard.
	/// </param>
	public static void AddBlacklistTournament(string tournamentId)
	{
		getINextpeerInstance().AddBlacklistTournament(tournamentId);
	}
	
	/// <summary>
	/// Removes the tournament from the blacklist.
	/// </summary>
	/// <param name='tournamentId'>
	/// The Tournament ID to remove from the blacklist.
	/// </param>
	public static void RemoveBlacklistTournament(string tournamentId)
	{
		getINextpeerInstance().RemoveBlacklistTournament(tournamentId);
	}
	
	/// <summary>
	/// Clears the tournament blacklist.
	/// </summary>
	public static void ClearTournamentBlacklist()
	{
		getINextpeerInstance().ClearTournamentBlacklist();
	}
	
	#endregion
	
	#endregion

	#region Recording manipulation
	
	/// <summary>
	/// Call this method to change the score of the given recording. 
	/// The modifier can be negative or positive and thus points will either be added or reduced from the recording’s score.
	/// </summary>
	/// <param name='userId'>
	/// The player ID of the target recording
	/// </param>
	///	<param name='scoreModifier'>
	/// The score modifire to apply to the recording
	/// </param>
	public static void ReportScoreModifier(String userId, Int32 scoreModifier)
	{
		getINextpeerInstance().ReportScoreModifier(userId, scoreModifier);
	}
	
	/// <summary>
	///	Call this method to fast forward the given recording by timeDelta milliseconds.
	/// </summary>
	/// <param name='userId'>
	/// The player ID of the target recording
	/// </param>
	///	<param name='timeDeltaMilliseconds'>
	/// The interval by which to fast-forward the recording
	/// </param>
	public static void RequestFastForwardRecording(String userId, TimeSpan timeDelta)
	{
		getINextpeerInstance().RequestFastForwardRecording(userId, timeDelta);
	}
	
	/// <summary>
	///	Call this method to pause the given recording.
	/// </summary>
	/// <param name='userId'>
	/// The player ID of the target recording
	/// </param>
	public static void RequestPauseRecording(String userId)
	{
		getINextpeerInstance().RequestPauseRecording(userId);
	}
	
	/// <summary>
	///	Call this method to resume the given recording.
	/// </summary>
	/// <param name='userId'>
	/// The player ID of the target recording
	/// </param>
	public static void RequestResumeRecording(String userId)
	{
		getINextpeerInstance().RequestResumeRecording(userId);
	}
	
	/// <summary>
	///	Call this method to rewind the given recording by timeDelta milliseconds.
	/// </summary>
	/// <param name='userId'>
	/// The player ID of the target recording
	/// </param>
	///	<param name='timeDelta'>
	/// The interval by which to rewind the recording
	/// </param>
	public static void RequestRewindRecording(String userId, TimeSpan timeDelta)
	{
		getINextpeerInstance().RequestRewindRecording(userId, timeDelta);
	}
	
	/// <summary>
	///	Call this method to stop the given recording
	/// </summary>
	/// <param name='userId'>
	/// The player ID of the target recording
	/// </param>
	public static void RequestStopRecording(String userId)
	{
		getINextpeerInstance().RequestStopRecording(userId);
	}
	
	#endregion
	
	#region Social

	/// <summary>
	/// Use this method to retrieve the current player details such as name and image.
	/// </summary>
	/// <returns>
	/// The current player details, or null if Nextpeer wasn't initalised.
	/// </returns>
	public static NPGamePlayerContainer GetCurrentPlayerDetails()
    {
        return getINextpeerInstance().GetCurrentPlayerDetails();
    }

	#endregion
	
	#region Configuration

	/// <summary>
	/// Call this method to enable or disable the in-game ranking display during a tournament.
	/// You can change this setting during a tournament (making the ranking display appear and disappear), but will not be animated.
	/// 
	/// Note: must only be called after Nextpeer's initailisation.
	/// </summary>
	/// <param name='enableRankingDisplay'>
	/// true to enable (and show) the ranking display, false to disable (and hide) it.
	/// </param>
	public static void EnableRankingDisplay(bool enableRankingDisplay)
	{
		getINextpeerInstance().EnableRankingDisplay(enableRankingDisplay);
	}

#if UNITY_IPHONE	
	/// <summary>
	/// You can use this method to set if Nextpeer will show an alert view if it is not supported on the current device.
	/// 
	/// Default is <c>false</c> (Nextpeer WILL display an error).
	/// 
	/// If you want to set this setting, call this method BEFORE calling Nextpeer.Init.
	/// </summary>
	/// <param name='ShowError'>
	/// <c>true</c> if Nextpeer SHOULD NOT show it's own error (and so you can display your own error), <false> if Nextpeer SHOULD display an error.
	/// </param>
	public static void SetNextpeerNotSupportedShouldShowCustomErrors(Boolean ShowError)
    {
        getINextpeerInstance().SetNextpeerNotSupportedShouldShowCustomErrors(ShowError);
    }
#endif

	#endregion

	#endregion
	
	#endregion
	
	#region Private interface
	
	private static INextpeer _nextpeer;
	private static bool _isInitialised = false;

	private static INextpeer getINextpeerInstance(){
		
		if (null == _nextpeer) {
			#if UNITY_ANDROID
			_nextpeer = new NextpeerAndroid ();
			#elif UNITY_IPHONE
			_nextpeer = new NextpeerIOS();
			#endif
		}
		
		return _nextpeer;
	}
	
	void Awake()
	{
		//Here we change the gameObject name to "Nextpeer" even though it named on creation, for backward compatibility -
		//to handle cases of users which have Nextpeer placeholder game object with different name.
		gameObject.name = this.GetType().ToString();
		DontDestroyOnLoad( this );
	}
	
#if UNITY_ANDROID
	void OnApplicationPause(bool pause)
	{
#if UNITY_EDITOR
		return;
#endif
		
		if (!pause)
		{
			((NextpeerAndroid)getINextpeerInstance()).onStart();
		}
		else
		{
			((NextpeerAndroid)getINextpeerInstance()).onStop(
				() => { DidTournamentEndHandler(); }
				);
		}
	}
	
	void OnApplicationQuit()
	{
#if UNITY_EDITOR
		return;
#endif
		
		((NextpeerAndroid)getINextpeerInstance()).onStop(
				() => { DidTournamentEndHandler(); }
				);
	}
#endif
	
	#region Message handlers
	
	private void DidTournamentStartWithDetailsHandler()
	{
		NPTournamentStartDataContainer _npTournamentStartDataContainer = getINextpeerInstance().GetTournamentStartData();
		if (WillTournamentStartWithDetails != null)
		{
			WillTournamentStartWithDetails(_npTournamentStartDataContainer);
		}

		if (DidTournamentStartWithDetails != null)
		{
			DidTournamentStartWithDetails(_npTournamentStartDataContainer);
		}
	}
	
	private void DidTournamentEndHandler()
	{
		if (DidTournamentEnd != null)
		{
			DidTournamentEnd();
		}
	}
	
	private void DashboardWillAppearHandler()
	{
		if (DashboardWillAppear != null)
		{
			DashboardWillAppear();
		}
	}
	
	private void DashboardDidAppearHandler()
	{
		if (DashboardDidAppear != null)
		{
			DashboardDidAppear();
		}
	}
	
	private void DashboardWillDisappearHandler()
	{
		if (DashboardWillDisappear != null)
		{
			DashboardWillDisappear();
		}
	}
	
	private void DashboardDidDisappearHandler()
	{
		if (DashboardDidDisappear != null)
		{
			DashboardDidDisappear();
		}
	}
	
	private void DashboardDidReturnToGameHandler()
	{
		if (DashboardDidReturnToGame != null)
		{
			DashboardDidReturnToGame();
		}
	}
	
	private void DidReceiveTournamentCustomMessageHandler(string messageId)
	{
		if (DidReceiveTournamentCustomMessage != null)
		{
			NPTournamentCustomMessageContainer message = getINextpeerInstance().ConsumeReliableCustomMessage(messageId);
		
			if (message != null)
			{
				DidReceiveTournamentCustomMessage(message);
			}
		}
		else
		{
			getINextpeerInstance().RemoveStoredObjectWithId(messageId);
		}
	}
	
	private void DidReceiveUnreliableTournamentCustomMessageHandler(string messageId)
	{
		if (DidReceiveUnreliableTournamentCustomMessage != null)
		{
			NPTournamentUnreliableCustomMessageContainer message = getINextpeerInstance().ConsumeUnreliableCustomMessage(messageId);
		
			if (message != null)
			{
				DidReceiveUnreliableTournamentCustomMessage(message);
			}
		}
		else
		{
			getINextpeerInstance().RemoveStoredObjectWithId(messageId);
		}
	}

	private void DidReceiveTournamentStatusInfoHandler(string objectId)
	{
		if (DidReceiveTournamentStatus != null)
		{
			NPTournamentStatusInfo? status = getINextpeerInstance().ConsumeTournamentStatusInfo(objectId);
			
			if (status != null)
			{
				DidReceiveTournamentStatus(status.Value);
			}
		}
		else
		{
			getINextpeerInstance().RemoveStoredObjectWithId(objectId);
		}
	}
	
	private void DidReceiveSynchronizedEventHandler(string objectId)
	{
		if (DidReceiveSynchronizedEvent != null)
		{
			string eventName = "";
			NPSynchronizedEventFireReason fireReason = NPSynchronizedEventFireReason.AllReached;
			if (getINextpeerInstance().ConsumeSyncEventInfo(objectId, ref eventName, ref fireReason))
			{
				DidReceiveSynchronizedEvent(eventName, fireReason);
			}
		}
		else
		{
			getINextpeerInstance().RemoveStoredObjectWithId(objectId);
		}
	}
	
#if UNITY_ANDROID
	private void DidReceiveTournamentResultsHandler()
	{
		if (DidReceiveTournamentResults != null)
		{
			DidReceiveTournamentResults(getINextpeerInstance().GetTournamentResult());
		}
	}
#endif

#if UNITY_ANDROID
	private IEnumerator RequestNextFrameNotifications()
    {
        Debug.Log("Frame notifications requested, waiting for end of frame");
        yield return new WaitForEndOfFrame();

		bool fbBound = getINextpeerInstance().FrameStart(Screen.width, Screen.height);

		if (fbBound) {
			foreach (Camera cam in Camera.allCameras) {
				if (!cam.enabled) {
					Debug.Log ("Skipping disabled camera " + cam.name + " (depth=" + cam.depth + ")");
				} else if (cam.targetTexture != null) {
					Debug.Log ("Skipping off-screen camera " + cam.name + " (depth=" + cam.depth + ")");
				} else {
					Debug.Log ("Rendering camera " + cam.name + " (depth=" + cam.depth + ")");
					cam.Render ();
				}
			}
		}

		getINextpeerInstance().FrameEnd();
    }
#endif
	
	#endregion
	
	#endregion
}

#endif
