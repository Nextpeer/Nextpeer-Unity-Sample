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

#if UNITY_IPHONE
	/// <summary>
	/// Whether or not the current player is a Facebook (or Twitter in the future for example) user or not.
	/// </summary>
    public bool IsSocialAuthenticated;
#endif
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
	/// The display name.
	/// </summary>
    public String DisplayName;
	
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
	
	[Obsolete("In-game notifications have been removed, so this setting no longer has any effect.")]
	/// <summary>
	/// Specifies if the game supports retina mode (iOS4+). This affects generated images that come
	/// from the NPNotificationContainer. If set to True, the generated images will be sized according to the
	/// device compatibility (retina devices receiving larger images).
	/// 
	/// Default: true
	/// </summary>
    public Boolean? SupportsRetina;
	
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
	/// The tournament duration in seconds (if the tournament is timed, i.e. not game controlled).
	/// </summary>
    public UInt32 TournamentTimeSeconds;
	
	/// <summary>
	/// A random seed generated for this tournament. All players within the same tournament
	/// receive the same seed from the tournament. Can be used for level generation, to ensure
	/// all players play the same level in a specific game.
	/// </summary>
    public UInt32 TournamentRandomSeed;
	
	/// <summary>
	/// A flag that marks if the current tournament is game controlled.
	/// </summary>
    public Boolean TournamentIsGameControlled;
	
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
	/// The player total currency amount (after the tournament ended).
	/// </summary>
	public UInt32 CurrentCurrencyAmount;
	
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

[Obsolete("In-game notifications have been removed, this struct is never used.")]
/// <summary>
/// Contains information about in-game notifications.
/// </summary>
public struct NPNotificationContainer
{
	/// <summary>
	/// The text of the notification.
	/// </summary>
	public String NotificationText;
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
public enum NPNotificationPosition:int
{
	/// <summary>
	/// Rankings are shown in the top-center of the screen
	/// </summary>
    NPNotificationPosition_TOP = 1,
	
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
	List = 1,
	
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
	Optimised = 1,
	
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
	Horizontal = 1,
	
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
	
	/// <summary>
	/// Occurs when Nextpeer will hide itself in order to allow the game to display a "inter-game" screen.
	/// Only relevant in conjunction with Nextpeer.SetShouldAllowInterGameScreen.
	/// </summary>
	public static event Action WillHideToShowInterGameScreen;
	
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

#if UNITY_IPHONE
	[Obsolete("In-game notifications have been removed (and replaced by in-game ranking display), this event will never be called.")]
	/// <summary>
	/// Occurs when an in-game notification arrives from the server and Nextpeer.SetNextpeerNotificationAllowed(false) was called.
	/// It is recommended to here display some sort of game specific version of the notification based on the notification data.
	/// </summary>
	public static event Action<NPNotificationContainer> HandleDisallowedNotification;
	
	[Obsolete("In-game notifications have been removed (and replaced by in-game ranking display), this event will never be called.")]
	/// <summary>
	/// Occurs when a notification is about to appear.
	/// </summary>
	public static event Action<NPNotificationContainer> NotificationWillShow;
	
	// Events corresponding to CurrencyDelegate:
	
	/// <summary>
	/// Occurs when the current player's currency changes due to an internal event.
	/// This may be a result of currency consumption (negative amount, for example when entering a tournament)
	/// or a currency gain (positive amount, when winning a tournament).
	/// </summary>
	public static event Action<int> AddAmountToCurrency;
#endif

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
		
		return _nextpeer.IsNextpeerSupported();
	}
	
	/// <returns>
	/// The release Version String of the Nextpeer client library in use.
	/// </returns>
	public static string ReleaseVersionString()
    {
        return _nextpeer.ReleaseVersionString();
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
		
		if (_isInitialised)
		{
			return;
		}
		
		_nextpeer.Init (GameKey, Settings);
		
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
		
        _nextpeer.LaunchDashboard();
    }

#if UNITY_IPHONE
	/// <summary>
	/// Closes the Nextpeer dashboard.
	/// </summary>
	public static void DismissDashboard()
    {
        _nextpeer.DismissDashboard();
    }
#endif
	
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
        return _nextpeer.IsCurrentlyInTournament();
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
        _nextpeer.ReportScoreForCurrentTournament(score);
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
        _nextpeer.ReportControlledTournamentOverWithScore(score);
    }
	
	/// <summary>
	/// Call this method when the user wishes to exit the current tournament prematurely.
	/// Note that this will count as a loss for the player, and will not re-launch the Nextpeer dashboard.
	/// To finish the tournament under regular conditions (e.g., player died in-game), use Nextpeer.ReportControlledTournamentOverWithScore.
	/// This will close any in-game notifiactions and dialogs.
	/// </summary>
	public static void ReportForfeitForCurrentTournament()
    {
        _nextpeer.ReportForfeitForCurrentTournament();
    }
	
	/// <summary>
	/// This method will return the amount of seconds left for this tournament.
	///
	/// If no tournament is currently taking place then this method will return 0.
	/// </summary>
	/// <returns>
	/// Time left in the tournament.
	/// </returns>
	public static TimeSpan TimeLeftInTournament()
    {
        return _nextpeer.TimeLeftInTournament();
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
        _nextpeer.PushDataToOtherPlayers(data);
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
        _nextpeer.UnreliablePushDataToOtherPlayers(data);
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
		_nextpeer.RegisterToSyncEvent(eventName, timeout);
	}

#if UNITY_IPHONE
	[Obsolete("In-game notifications have been repalced by in-game ranking display, so this method no longer has any effect. As an alterantive, consider using PushDataToOtherPlayers.")]
	/// <summary>
	///  This method will broadcast an in-game notification to the other players in the tournament.
    /// The current player's image will be displayed along with the text.
	/// 
	/// To use the current player's name in the message use %PLAYER_NAME%.
	/// E.g., "%PLAYER_NAME% sent you a bomb!"
	/// </summary>
	/// <param name='message'>
	/// The message to send.
	/// </param>
	public static void PushMessageToOtherPlayers(String message)
    {
    }
#endif
	
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
		_nextpeer.AddWhitelistTournament(tournamentId);
	}
	
	/// <summary>
	/// Removes the tournament from the whitelist.
	/// </summary>
	/// <param name='tournamentId'>
	/// The tournament ID to remove from the whitelist.
	/// </param>
	public static void RemoveWhitelistTournament(string tournamentId)
	{
		_nextpeer.RemoveWhitelistTournament(tournamentId);
	}
	
	/// <summary>
	/// Clears the tournament whitelist.
	/// </summary>
	public static void ClearTournamentWhitelist()
	{
		_nextpeer.ClearTournamentWhitelist();
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
		_nextpeer.AddBlacklistTournament(tournamentId);
	}
	
	/// <summary>
	/// Removes the tournament from the blacklist.
	/// </summary>
	/// <param name='tournamentId'>
	/// The Tournament ID to remove from the blacklist.
	/// </param>
	public static void RemoveBlacklistTournament(string tournamentId)
	{
		_nextpeer.RemoveBlacklistTournament(tournamentId);
	}
	
	/// <summary>
	/// Clears the tournament blacklist.
	/// </summary>
	public static void ClearTournamentBlacklist()
	{
		_nextpeer.ClearTournamentBlacklist();
	}
	
	#endregion
	
	#endregion
	
	#region Social

#if UNITY_IPHONE
	/// <summary>
	/// Use this method to invoke the Facebook post dialog.
	/// The user will be prompted to login if she hasn't done that before.
	/// </summary>
	/// <param name='message'>
	/// Message to be displayed on the wall. Must not be null.
	/// </param>
	/// <param name='link'>
	/// Link for the given post. Could link to anywhere. If null then the link would be to the app's iTunes page (what was specified in Nextpeer's dashboard).
	/// </param>
	/// <param name='ImageUrl'>
	/// URL for an image to be displayed on the post. If null then the image is the app's icon as it appears in Nextpeer's dashboard.
	/// </param>
	public static void PostToFacebookWall(String message, String link, String ImageUrl)
    {
        _nextpeer.PostToFacebookWall(message, link, ImageUrl);
    }
#endif

	/// <summary>
	/// Use this method to retrieve the current player details such as name and image.
	/// </summary>
	/// <returns>
	/// The current player details, or null if Nextpeer wasn't initalised.
	/// </returns>
	public static NPGamePlayerContainer GetCurrentPlayerDetails()
    {
        return _nextpeer.GetCurrentPlayerDetails();
    }

#if UNITY_IPHONE
	/// <summary>
	/// Use this method to open Nextpeer's game stream.
	/// </summary>
	public static void OpenFeedDashboard()
    {
        _nextpeer.OpenFeedDashboard();
    }
#endif

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
		_nextpeer.EnableRankingDisplay(enableRankingDisplay);
	}

#if UNITY_IPHONE
	[Obsolete("Notification orientation should be set via the settings. See NPGameSettings.NotificationOrientation and NPGameSettings.ObserveNotificationOrientationChange.")]
	/// <summary>
	/// Call this method when you wish to change the in-game notification orientation in run time.
	/// It's preferable to use the <c>NPGameSettings</c> struct if you wish to set this up at start-time.
	/// </summary>
	/// <param name='Orientation'>
	/// The orientation at which in-game notifications will display.
	/// </param>
	public static void SetNotificationOrientation(NPUIInterfaceOrientation Orientation)
    {
		
    }
	
	[Obsolete("In-game notifications have been removed. This method has no effect. " +
		"In-game notifications have been replaced by the in-game ranking display. To control it, see EnableRankingDisplay(bool).")]
	/// <summary>
	/// Tells Nextpeer if it should display in-game notifications received from the server.
	/// 
	/// If you turn in-game notifications off, you can reigster to the Nextpeer.HandleDisallowedNotification event,
	/// and display your own custom in-game notification.
	/// </summary>
	/// <param name='isAllowed'>
	/// <c>true</c> to allow Nextpeer to display in-game notifications (default), <c>false</c> to disallow it.
	/// </param>
	public static void SetNextpeerNotificationAllowed(Boolean isAllowed)
    {
        
    }
	
	/// <summary>
	/// Tells Nextpeer if the game wants to display its own screens immediately after the player taps the "Play Again" button and before the next game starts.
	/// If you implement this method, you should also register to the Nextpeer.WillHideToShowInterGameScreen event.
	/// </summary>
	/// <param name='allowInterGameScreen'>
	/// <c>true</c> to allow to display an inter-game screen, <c>false</c> otherwise.
	/// </param>
	public static void SetAllowInterGameScreen(Boolean allowInterGameScreen)
	{
		_nextpeer.SetAllowInterGameScreen(allowInterGameScreen);
	}
	
	/// <summary>
	/// Call this method when you have finished running the inter-game logic. The player will be taken to their next tournament.
	/// </summary>
	public static void ResumePlayAgainLogic()
	{
		_nextpeer.ResumePlayAgainLogic();
	}
	
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
        _nextpeer.SetNextpeerNotSupportedShouldShowCustomErrors(ShowError);
    }
#endif

	#endregion

	#region Currency

	#if UNITY_IPHONE
	
	/// <summary>
	/// Gets the currency amount, as stored by Nextpeer.
	/// </summary>
	/// <returns>
	/// The current amount of currency that Nextpeer has for the current player.
	/// </returns>
	public static Int32 GetCurrencyAmount()
    {
    	return _nextpeer.GetCurrencyAmount();
    }
	
	/// <summary>
	/// Sets the currency amount for the current player. Note that this will overwrite the previous currency amount.
	/// </summary>
	/// <param name='amount'>
	/// The currency amount of the current player.
	/// </param>
	public static void SetCurrencyAmount(Int32 amount)
    {
        _nextpeer.SetCurrencyAmount(amount);
    }
	
	/// <summary>
	/// Tells Nextpeer if the game supports the unified currency model. Should be called once before Nextpeer.Init.
	/// </summary>
	/// <param name='supported'>
	/// <c>true</c> if the game supports the unified currency model; otherwise, <c>false</c>.
	/// </param>
	public static void SetSupportsUnifiedCurrency(Boolean supported)
    {
        _nextpeer.SetSupportsUnifiedCurrency(supported);
    }

	#endif

	#endregion


	#endregion
	
	#endregion
	
	#region Private interface
	
	private static INextpeer _nextpeer;
	private static bool _isInitialised = false;
	
	void Awake()
	{
		// Set the GameObject name to the class name for easy access from Obj-C
		gameObject.name = this.GetType().ToString();
		DontDestroyOnLoad( this );

#if UNITY_ANDROID
		_nextpeer = new NextpeerAndroid();
#elif UNITY_IPHONE
		_nextpeer = new NextpeerIOS();
#endif
	}
	
#if UNITY_ANDROID
	void OnApplicationPause(bool pause)
	{
#if UNITY_EDITOR
		return;
#endif
		
		if (!pause)
		{
			((NextpeerAndroid)_nextpeer).onStart();
		}
		else
		{
			((NextpeerAndroid)_nextpeer).onStop(
				() => { DidTournamentEndHandler(); }
				);
		}
	}
	
	void OnApplicationQuit()
	{
#if UNITY_EDITOR
		return;
#endif
		
		((NextpeerAndroid)_nextpeer).onStop(
				() => { DidTournamentEndHandler(); }
				);
	}
#endif
	
	#region Message handlers
	
	private void DidTournamentStartWithDetailsHandler()
	{
		if (DidTournamentStartWithDetails != null)
		{
			DidTournamentStartWithDetails(_nextpeer.GetTournamentStartData());
		}
	}
	
	private void DidTournamentEndHandler()
	{
		if (DidTournamentEnd != null)
		{
			DidTournamentEnd();
		}
	}
	
	private void WillTournamentStartWithDetailsHandler()
	{
		if (WillTournamentStartWithDetails != null)
		{
			WillTournamentStartWithDetails(_nextpeer.GetTournamentStartData());
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
	
	private void WillHideToShowInterGameScreenHandler()
	{
		if (WillHideToShowInterGameScreen != null)
		{
			WillHideToShowInterGameScreen();
		}
	}
	
	private void DidReceiveTournamentCustomMessageHandler(string messageId)
	{
		if (DidReceiveTournamentCustomMessage != null)
		{
			NPTournamentCustomMessageContainer message = _nextpeer.ConsumeReliableCustomMessage(messageId);
		
			if (message != null)
			{
				DidReceiveTournamentCustomMessage(message);
			}
		}
		else
		{
			_nextpeer.RemoveStoredObjectWithId(messageId);
		}
	}
	
	private void DidReceiveUnreliableTournamentCustomMessageHandler(string messageId)
	{
		if (DidReceiveUnreliableTournamentCustomMessage != null)
		{
			NPTournamentUnreliableCustomMessageContainer message = _nextpeer.ConsumeUnreliableCustomMessage(messageId);
		
			if (message != null)
			{
				DidReceiveUnreliableTournamentCustomMessage(message);
			}
		}
		else
		{
			_nextpeer.RemoveStoredObjectWithId(messageId);
		}
	}

	private void DidReceiveTournamentStatusInfoHandler(string objectId)
	{
		if (DidReceiveTournamentStatus != null)
		{
			NPTournamentStatusInfo? status = _nextpeer.ConsumeTournamentStatusInfo(objectId);
			
			if (status != null)
			{
				DidReceiveTournamentStatus(status.Value);
			}
		}
		else
		{
			_nextpeer.RemoveStoredObjectWithId(objectId);
		}
	}

	private void DidReceiveTournamentResultsHandler()
	{
		if (DidReceiveTournamentResults != null)
		{
			DidReceiveTournamentResults(_nextpeer.GetTournamentResult());
		}
	}
	
	private void DidReceiveSynchronizedEventHandler(string objectId)
	{
		if (DidReceiveSynchronizedEvent != null)
		{
			string eventName = "";
			NPSynchronizedEventFireReason fireReason = NPSynchronizedEventFireReason.AllReached;
			if (_nextpeer.ConsumeSyncEventInfo(objectId, ref eventName, ref fireReason))
			{
				DidReceiveSynchronizedEvent(eventName, fireReason);
			}
		}
		else
		{
			_nextpeer.RemoveStoredObjectWithId(objectId);
		}
	}

#if UNITY_IPHONE
	private void AddAmountToCurrencyHandler(string currencyDelta)
	{
		if (AddAmountToCurrency != null)
		{
			AddAmountToCurrency(int.Parse(currencyDelta));
		}
	}
#endif
	
	#endregion
	
	#endregion
}

#endif
