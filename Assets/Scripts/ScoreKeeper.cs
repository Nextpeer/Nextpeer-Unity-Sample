using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using SRandom = System.Random;

public class ScoreKeeper:MonoBehaviour
{
    public int carrying;
    public int carryLimit;
    public int deposited;
    public int winScore;
    
    public int gameLength;

    public GameObject guiMessage;

    public int TotalScore;

    public GUIText carryingGui;
    public GUIText depositedGui;
    public GUIText timerGui;
    public GUIText totalScoreGui;

    public List<AudioClip> collectSounds;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip pickupSound;
    public AudioClip depositSound;

    private float timeSinceLastPlay;
    private float timeLeft;

    private NextpeerGameManager NPManager = NextpeerGameManager.GetInstance();
    private int LastManStandingTime = 30;

    private Rect ForfeitButton;

    public void Awake()
    {
        ForfeitButton = new Rect(Screen.width - 100,
                                 0, 100, 60);
    }

    public void Start()
    {
        Debug.Log("start");

        if (NPManager.TournamentType == GameType.LastManStanding)
            gameLength = LastManStandingTime;
		else if (NPManager.TournamentType != GameType.Solo)
            gameLength = Nextpeer.TimeLeftInTournament().Seconds;
        else
            gameLength = 10;

        timeLeft = gameLength;
        timeSinceLastPlay = Time.time;

        UpdateCarryingGui();
        UpdateDepositedGui();
        UpdateTotalScoreGui();
        StartCoroutine(CheckTime());
		
		carryingGui.pixelOffset = new Vector2((Screen.width - carryingGui.GetScreenRect().width)/2, carryingGui.pixelOffset.y);
		depositedGui.pixelOffset = new Vector2((Screen.width - depositedGui.GetScreenRect().width)/2, depositedGui.pixelOffset.y);
		totalScoreGui.pixelOffset = new Vector2((Screen.width - totalScoreGui.GetScreenRect().width)/2, totalScoreGui.pixelOffset.y);
		timerGui.pixelOffset = new Vector2((Screen.width - timerGui.GetScreenRect().width)/2, timerGui.pixelOffset.y);
    }

    private void UpdateCarryingGui()
    {
        carryingGui.text = "Carrying: " + carrying + " of " + carryLimit;
    }

    private void UpdateDepositedGui()
    {
        depositedGui.text = "Deposited: " + deposited + " of " + winScore;
    }

    private void UpdateTotalScoreGui()
    {
        totalScoreGui.text = "Total Score: " + TotalScore;
    }

    private void UpdateTimerGui()
    {
        timerGui.text = "Time: " + TimeRemaining();
    }

    private IEnumerator CheckTime()
    {
	    // Rather than using Update(), use a co-routine that controls the timer.
	    // We only need to check the timer once every second, not multiple times
	    // per second.
	    while ( timeLeft > 0 )
	    {
		    UpdateTimerGui();		
		    yield return new WaitForSeconds(1);
		    timeLeft -= 1;
	    }
	    UpdateTimerGui();
	    EndGame();
    }


    private void EndGame()
    {

	    GameObject prefab = (GameObject)Instantiate(guiMessage);
	    GUIText endMessage = prefab.GetComponent<GUIText>();
        endMessage.text = "";

	    // Alert other components on this GameObject that the game has ended
	    SendMessage( "OnEndGame" );

        if (NPManager.TournamentType == GameType.LastManStanding)
        {
            Nextpeer.ReportControlledTournamentOverWithScore((uint)TotalScore);
        }
    }

    public void Pickup(ParticlePickup pickup)
    {
	    if ( carrying < carryLimit )
	    {
	 	    carrying++;
		    UpdateCarryingGui();
            UpdateTotalScoreGui();
            TotalScore++;
            if(NextpeerGameManager.GetInstance().TournamentType != GameType.Solo)
                Nextpeer.ReportScoreForCurrentTournament((uint)TotalScore);

            if (NPManager.TournamentType == GameType.LastManStanding)
            {
                timeLeft += 1;
            }

		    // We don't want a voice played for every pickup as this would be annoying.
		    // Only allow a voice to play with a random percentage of chance and only
		    // after a minimum time has passed.
		    var minTimeBetweenPlays = 5;
		    if ( Random.value < 0.1 && Time.time > ( minTimeBetweenPlays + timeSinceLastPlay ) )
		    {
			    PlayAudioClip( collectSounds[ Random.Range( 0, collectSounds.Count) ], Vector3.zero, 0.25f );
			    timeSinceLastPlay = Time.time;
		    }
		
	 	    pickup.Collected();	
		    PlayAudioClip( pickupSound, pickup.transform.position, 1.0f );
	    }
	    else
	    {
		    GameObject warning = (GameObject) Instantiate( guiMessage );
		    warning.guiText.text = "You can't carry any more";
		    Destroy(warning, 2);
	    }
	
	    // Show the player where to deposit the orbs
 	    if ( carrying >= carryLimit )
		    pickup.emitter.SendMessage( "ActivateDepository" );	 	
    }

    public void Deposit()
    {
        TotalScore += carrying;
        if (NextpeerGameManager.GetInstance().TournamentType != GameType.Solo)
		{
            Nextpeer.ReportScoreForCurrentTournament((uint)TotalScore);
			NPManager.LastReportedScore = (uint)TotalScore;
		}

        if (NPManager.TournamentType == GameType.LastManStanding)
        {
            timeLeft += carrying;
        }
        deposited += carrying;
        carrying = 0;
        UpdateCarryingGui();
        UpdateDepositedGui();
        UpdateTotalScoreGui();
        PlayAudioClip(depositSound, transform.position, 1.0f);
    }


    public String TimeRemaining()
    {
	    int remaining = (int) timeLeft;
	    String val = "";
	    if(remaining > 59) // Insert # of minutes
	     val += remaining / 60 + ".";
	
	    if(remaining >= 0) // Add # of seconds
	    {
		    String seconds = (remaining % 60).ToString();
		    if(seconds.Length < 2)
			    val += "0" + seconds; // insert leading 0
		    else
			    val += seconds;
	    }
	    return val;
    }

    AudioSource PlayAudioClip(AudioClip clip,
                        Vector3 position,
                        float volume)
    {
        var go = new GameObject("One shot audio");
        go.transform.position = position;
        AudioSource source = go.AddComponent<AudioSource>();
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.clip = clip;
        source.volume = volume;
        source.Play();
        Destroy(go, clip.length);
        return source;
    }

}